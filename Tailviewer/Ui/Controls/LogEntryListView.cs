﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Converters;

namespace Tailviewer.Ui.Controls
{
	public class LogEntryListView
		: Grid
		, ILogFileListener
	{
		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof (ILogFile), typeof (LogEntryListView),
			                            new PropertyMetadata(null, OnLogFileChanged));

		public static readonly DependencyProperty ForegroundBrushConverterProperty =
			DependencyProperty.Register("ForegroundBrushConverter", typeof (LevelToBrushConverter), typeof (LogEntryListView),
			                            new PropertyMetadata(default(LevelToBrushConverter)));

		public static readonly DependencyProperty BackgroundBrushConverterProperty =
			DependencyProperty.Register("BackgroundBrushConverter", typeof (LevelToBrushConverter), typeof (LogEntryListView),
			                            new PropertyMetadata(default(LevelToBrushConverter)));

		public static readonly DependencyProperty HoveredBackgroundBrushConverterProperty =
			DependencyProperty.Register("HoveredBackgroundBrushConverter", typeof (LevelToBrushConverter),
			                            typeof (LogEntryListView), new PropertyMetadata(default(LevelToBrushConverter)));

		internal static readonly TimeSpan MaximumRefreshInterval = TimeSpan.FromMilliseconds(33);

		private readonly ScrollBar _horizontalScrollBar;
		private readonly ScrollBar _verticalScrollBar;
		private readonly DispatcherTimer _timer;

		static LogEntryListView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogEntryListView),
			                                         new FrameworkPropertyMetadata(typeof (LogEntryListView)));
		}

		public LogEntryListView()
		{
			_verticalScrollBar = new ScrollBar
				{
					Name = "PART_VerticalScrollBar",
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Right,
					Margin = new Thickness(0, 0, 0, 17)
				};
			_verticalScrollBar.ValueChanged += VerticalScrollBarOnScroll;

			_horizontalScrollBar = new ScrollBar
				{
					Name="PART_HorizontalScrollBar",
					VerticalAlignment=VerticalAlignment.Bottom,
					HorizontalAlignment=HorizontalAlignment.Stretch,
					Orientation = Orientation.Horizontal,
					Margin = new Thickness(0, 0, 17, 0)
				};
			_horizontalScrollBar.ValueChanged += HorizontalScrollBarOnScroll;

			Children.Add(_verticalScrollBar);
			Children.Add(_horizontalScrollBar);
			Children.Add(new Rectangle
				{
					Width = 17,
					Height = 17,
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Right,
					Fill = new SolidColorBrush(Color.FromRgb(0xce, 0xce, 0xce))
				});

			InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelUp), MouseWheelGesture.WheelUp));
			InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelDown), MouseWheelGesture.WheelDown));

			_visibleTextLines = new List<TextLine>();
			_timer = new DispatcherTimer(MaximumRefreshInterval, DispatcherPriority.Normal, OnTimer, Dispatcher);
			_timer.Start();

			ClipToBounds = true;

			SizeChanged += OnSizeChanged;
		}

		internal int PendingModificationsCount
		{
			get
			{
				return _pendingModificationsCount;
			}
		}

		private void OnTimer(object sender, EventArgs e)
		{
			if (Interlocked.Exchange(ref _pendingModificationsCount, 0) > 0)
			{
				// TODO: Optimize if performance drops below acceptable rates
				DetermineVerticalOffset();
				_currentlyVisibleSection = CalculateVisibleSection();
				UpdateScrollViewerRegions();
				UpdateVisibleLines();
			}
		}

		internal void OnMouseWheelDown()
		{
			var delta = _verticalScrollBar.Maximum - _verticalScrollBar.Value;
			var toScroll = Math.Min(delta, TextLine.LineHeight);
			_verticalScrollBar.Value += toScroll;
		}

		internal void OnMouseWheelUp()
		{
			var delta = _verticalScrollBar.Value - _verticalScrollBar.Minimum;
			var toScroll = Math.Min(delta, TextLine.LineHeight);
			_verticalScrollBar.Value -= toScroll;
		}

		internal ScrollBar VerticalScrollBar
		{
			get { return _verticalScrollBar; }
		}

		internal ScrollBar HorizontalScrollBar
		{
			get { return _horizontalScrollBar; }
		}

		public LevelToBrushConverter HoveredBackgroundBrushConverter
		{
			get { return (LevelToBrushConverter) GetValue(HoveredBackgroundBrushConverterProperty); }
			set { SetValue(HoveredBackgroundBrushConverterProperty, value); }
		}

		public LevelToBrushConverter BackgroundBrushConverter
		{
			get { return (LevelToBrushConverter) GetValue(BackgroundBrushConverterProperty); }
			set { SetValue(BackgroundBrushConverterProperty, value); }
		}

		public LevelToBrushConverter ForegroundBrushConverter
		{
			get { return (LevelToBrushConverter) GetValue(ForegroundBrushConverterProperty); }
			set { SetValue(ForegroundBrushConverterProperty, value); }
		}

		public ILogFile LogFile
		{
			get { return (ILogFile) GetValue(LogFileProperty); }
			set { SetValue(LogFileProperty, value); }
		}

		public double ActualViewerHeight
		{
			get
			{
				var height = ActualHeight;
				if (_horizontalScrollBar.Visibility != Visibility.Collapsed)
				{
					var usableHeight = height - _horizontalScrollBar.ActualHeight;
					return usableHeight;
				}

				return height;
			}
		}

		/// <summary>
		///     The section of the log file that is currently visible.
		/// </summary>
		[Pure]
		public LogFileSection CalculateVisibleSection()
		{
			return CalculateVisibleSection(LogFile);
		}

		[Pure]
		private LogFileSection CalculateVisibleSection(ILogFile logFile)
		{
			if (logFile == null)
				return new LogFileSection(0, 0);

			double maxLinesInViewport = (ActualHeight + _yOffset)/TextLine.LineHeight;
			var maxCount = (int) Math.Ceiling(maxLinesInViewport);
			int linesLeft = LogFile.Count - _currentLine;
			int count = Math.Min(linesLeft, maxCount);
			return new LogFileSection(_currentLine, count);
		}

		#region Updates

		private int _pendingModificationsCount;

		#endregion

		#region Cached data

		private readonly List<TextLine> _visibleTextLines;
		private int _currentLine;
		private LogFileSection _currentlyVisibleSection;
		private TextLine _hoveredLine;
		private TextLine _selectedLine;
		private double _yOffset;

		#endregion

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			DetermineVerticalOffset();
			LogFileSection previousVisibleSection = _currentlyVisibleSection;
			_currentlyVisibleSection = CalculateVisibleSection();
			UpdateScrollViewerRegions();
			UpdateVisibleLines(LogFile, previousVisibleSection, _currentlyVisibleSection);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			Point relativePos = e.GetPosition(this);
			var lineIndex = (int) Math.Floor(relativePos.Y/TextLine.LineHeight);
			if (lineIndex >= 0 && lineIndex < _visibleTextLines.Count)
			{
				TextLine hoveredLine = _visibleTextLines[lineIndex];
				if (hoveredLine != _hoveredLine)
				{
					hoveredLine.IsHovered = true;
					if (_hoveredLine != null)
					{
						_hoveredLine.IsHovered = false;
					}
					_hoveredLine = hoveredLine;

					if (Mouse.LeftButton == MouseButtonState.Pressed)
						TrySelectLine(_hoveredLine);

					InvalidateVisual();
				}
			}
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (_hoveredLine != null)
			{
				_hoveredLine.IsHovered = false;
				_hoveredLine = null;
				InvalidateVisual();
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (TrySelectLine(_hoveredLine))
			{
				e.Handled = true;
				InvalidateVisual();
			}

			base.OnMouseLeftButtonDown(e);
		}

		private bool TrySelectLine(TextLine line)
		{
			if (line != null)
			{
				if (_selectedLine != null)
					_selectedLine.IsSelected = false;

				_selectedLine = line;
				_selectedLine.IsSelected = true;

				return true;
			}

			return false;
		}

		private static void OnLogFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnLogFileChanged(e.OldValue as ILogFile, e.NewValue as ILogFile);
		}

		private void OnLogFileChanged(ILogFile oldValue, ILogFile newValue)
		{
			if (oldValue != null)
			{
				oldValue.RemoveListener(this);
			}

			_visibleTextLines.Clear();
			if (newValue != null)
			{
				newValue.AddListener(this, TimeSpan.FromMilliseconds(100), 10000);

				DetermineVerticalOffset();
				_currentLine = 0;
				_currentlyVisibleSection = CalculateVisibleSection(newValue);
				UpdateScrollViewerRegions();
				UpdateVisibleLines(newValue);
			}
		}

		private void UpdateVisibleLines()
		{
			UpdateVisibleLines(LogFile);
		}

		private void UpdateVisibleLines(ILogFile logFile)
		{
			_visibleTextLines.Clear();
			if (logFile == null)
				return;

			var data = new LogLine[_currentlyVisibleSection.Count];
			logFile.GetSection(_currentlyVisibleSection, data);
			for (int i = 0; i < _currentlyVisibleSection.Count; ++i)
			{
				_visibleTextLines.Add(new TextLine(data[i]));
			}

			InvalidateVisual();
		}

		private void UpdateScrollViewerRegions()
		{
			if (LogFile == null)
			{
			}
			else
			{
				//
				// Depending on the current size of the logfile and the size of the viewport,
				// we need to determine the range of our scrollers.
				//

				int count = LogFile.Count;
				double totalHeight = count*TextLine.LineHeight;
				double usableHeight = ActualViewerHeight;
				if (totalHeight > usableHeight)
				{
					_verticalScrollBar.Minimum = 0;
					_verticalScrollBar.Maximum = totalHeight - usableHeight;
					_verticalScrollBar.ViewportSize = usableHeight;
				}
				else
				{
					_verticalScrollBar.Minimum = 0;
					_verticalScrollBar.Maximum = 0;
					_verticalScrollBar.ViewportSize = usableHeight;
				}
			}
		}

		private void DetermineVerticalOffset()
		{
			var value = _verticalScrollBar.Value;
			var lineBeginning = (int) (Math.Floor(value/TextLine.LineHeight)*TextLine.LineHeight);
			_yOffset = lineBeginning - value;
		}

		private void UpdateVisibleLines(ILogFile logFile, LogFileSection oldSection, LogFileSection newSection)
		{
			UpdateVisibleLines(logFile);
		}

		private void VerticalScrollBarOnScroll(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			double pos = args.NewValue;
			var currentLine = (int) Math.Floor(pos/TextLine.LineHeight);

			DetermineVerticalOffset();
			_currentLine = currentLine;
			_currentlyVisibleSection = CalculateVisibleSection();
			UpdateVisibleLines();
		}

		private void HorizontalScrollBarOnScroll(object sender, RoutedPropertyChangedEventArgs<double> ars)
		{
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			var rect = new Rect(0, 0, ActualWidth, ActualHeight);
			drawingContext.DrawRectangle(Brushes.White, null, rect);

			double y = _yOffset;
			foreach (TextLine textLine in _visibleTextLines)
			{
				textLine.Render(drawingContext, y, ActualWidth);
				y += TextLine.LineHeight;
			}
		}

		public List<TextLine> VisibleTextLines
		{
			get { return _visibleTextLines; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			Interlocked.Increment(ref _pendingModificationsCount);
		}
	}
}