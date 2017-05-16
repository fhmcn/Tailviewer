﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.Controls.SidePanel
{
	internal sealed class BookmarksViewModel
		: AbstractSidePanelViewModel
	{
		private readonly DelegateCommand _addBookmarkCommand;
		private readonly ObservableCollection<BookmarkViewModel> _bookmarks;
		private readonly Dictionary<Bookmark, BookmarkViewModel> _viewModelByBookmark;
		private readonly IDataSources _dataSources;
		private readonly Action<Bookmark> _navigateToBookmark;

		private IDataSource _currentDataSource;
		private bool _canAddBookmarks;

		public BookmarksViewModel(IDataSources dataSources, Action<Bookmark> navigateToBookmark)
		{
			if (dataSources == null)
				throw new ArgumentNullException(nameof(dataSources));
			if (navigateToBookmark == null)
				throw new ArgumentNullException(nameof(navigateToBookmark));

			_dataSources = dataSources;
			_navigateToBookmark = navigateToBookmark;
			_addBookmarkCommand = new DelegateCommand(AddBookmark, CanAddBookmark);
			_viewModelByBookmark = new Dictionary<Bookmark, BookmarkViewModel>();
			_bookmarks = new ObservableCollection<BookmarkViewModel>();
		}

		public ICommand AddBookmarkCommand => _addBookmarkCommand;

		private bool CanAddBookmarks
		{
			get { return _canAddBookmarks; }
			set
			{
				if (value == _canAddBookmarks)
					return;

				_canAddBookmarks = value;
				_addBookmarkCommand.RaiseCanExecuteChanged();
			}
		}

		public IEnumerable<BookmarkViewModel> Bookmarks => _bookmarks;

		public IDataSource CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				EmitPropertyChanged();
			}
		}

		public override Geometry Icon => Icons.Bookmark;

		public override string Id => "bookmarks";

		public override void Update()
		{
			var bookmarks = _dataSources.Bookmarks;
			if (bookmarks != null)
			{
				// Add bookmarks we haven't added yet
				foreach (var bookmark in bookmarks)
				{
					BookmarkViewModel viewModel;
					if (!_viewModelByBookmark.TryGetValue(bookmark, out viewModel))
					{
						viewModel = new BookmarkViewModel(bookmark,
							OnNavigateToBookmark,
							OnRemoveBookmark);

						_viewModelByBookmark.Add(bookmark, viewModel);
						Insert(viewModel);
					}
				}

				// Remove bookmarks that shouldn't be there
				for (int i = 0; i < _bookmarks.Count;)
				{
					var viewModel = _bookmarks[i];
					if (!bookmarks.Contains(viewModel.Bookmark))
					{
						_viewModelByBookmark.Remove(viewModel.Bookmark);
						_bookmarks.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}
			CanAddBookmarks = Any(_currentDataSource?.SelectedLogLines);
		}

		private void OnNavigateToBookmark(BookmarkViewModel viewModel)
		{
			_navigateToBookmark(viewModel.Bookmark);
		}

		private void OnRemoveBookmark(BookmarkViewModel viewModel)
		{
			_viewModelByBookmark.Remove(viewModel.Bookmark);
			_bookmarks.Remove(viewModel);
			_dataSources.RemoveBookmark(viewModel.Bookmark);
		}

		private static bool Any(HashSet<LogLineIndex> selectedLogLines)
		{
			return selectedLogLines?.Count > 0;
		}

		private void Insert(BookmarkViewModel viewModel)
		{
			int i;
			for (i = 0; i < _bookmarks.Count; ++i)
			{
				var bookmark = _bookmarks[i];
				if (bookmark.Bookmark.Index > viewModel.Bookmark.Index)
					break;
			}
			_bookmarks.Insert(i, viewModel);
		}

		private bool CanAddBookmark()
		{
			return CanAddBookmarks;
		}

		private void AddBookmark()
		{
			var dataSource = _currentDataSource;
			var lines = _currentDataSource?.SelectedLogLines;
			if (dataSource == null || lines == null)
				return;

			foreach (var line in lines)
			{
				var bookmark = _dataSources.TryAddBookmark(dataSource, line);
				if (bookmark != null)
				{
					var viewModel = new BookmarkViewModel(bookmark, OnNavigateToBookmark, OnRemoveBookmark);
					_viewModelByBookmark.Add(bookmark, viewModel);
					Insert(viewModel);
				}
			}
		}
	}
}