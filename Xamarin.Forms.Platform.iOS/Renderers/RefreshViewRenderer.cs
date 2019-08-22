﻿using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Platform.iOS
{
	public class RefreshViewRenderer : ViewRenderer<RefreshView, UIView>, IEffectControlProvider
	{
		bool _isDisposed;
		bool _isRefreshing;
		bool _usingLargeTitles;
		nfloat _origininalY;
		nfloat _refreshControlHeight;
		UIView _refreshControlParent;
		UIRefreshControl _refreshControl;

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;

				if (_isRefreshing)
					_refreshControl.BeginRefreshing();
				else
					_refreshControl.EndRefreshing();

				TryOffsetRefresh(this, IsRefreshing);
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<RefreshView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null || Element == null)
				return;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					if (Forms.IsiOS11OrNewer)
					{
						var parentNav = e.NewElement.FindParentOfType<NavigationPage>();
						_usingLargeTitles = parentNav != null && parentNav.OnThisPlatform().PrefersLargeTitles();
					}

					_refreshControl = new UIRefreshControl();
					_refreshControl.ValueChanged += OnRefresh;
					_refreshControlParent = this;
				}
			}

			UpdateColors();
			UpdateIsRefreshing();
			UpdateIsEnabled();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == RefreshView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.IsOneOf(RefreshView.RefreshColorProperty, VisualElement.BackgroundColorProperty))
				UpdateColors();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (_refreshControl == null)
				return;

			_refreshControl.BackgroundColor = color != Color.Default ? color.ToUIColor() : null;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing && Control != null)
			{
				_refreshControl.ValueChanged -= OnRefresh;
				_refreshControl.Dispose();
				_refreshControl = null;
				_refreshControlParent = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		bool TryOffsetRefresh(UIView view, bool refreshing)
		{
			if (view is UITableView)
			{
				var tableView = view as UITableView;

				if (tableView.ContentOffset.Y < 0)
					return true;
			
				if (refreshing)
					tableView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY - _refreshControlHeight), true);
				else
					tableView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY), true);

				return true;
			}

			if (view is UICollectionView)
			{
				var collectionView = view as UICollectionView;

				if (collectionView.ContentOffset.Y < 0)
					return true;

				if (refreshing)
					collectionView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY - _refreshControlHeight), true);
				else
					collectionView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY), true);

				return true;
			}

			if (view is UIWebView)
			{
				return true;
			}

			if (view is UIScrollView)
			{
				var scrollView = view as UIScrollView;

				if (scrollView.ContentOffset.Y < 0)
					return true;

				if (refreshing)
					scrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY - _refreshControlHeight), true);
				else
					scrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY), true);

				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryOffsetRefresh(control, refreshing))
					return true;
			}

			return false;
		}

		bool TryInsertRefresh(UIView view, int index = 0)
		{
			_refreshControlParent = view;

			if (view is UITableView)
			{
				var tableView = view as UITableView;

				if (CanUseRefreshControlProperty())
					tableView.RefreshControl = _refreshControl;
				else
					tableView.InsertSubview(_refreshControl, index);

				_origininalY = tableView.ContentOffset.Y;
				_refreshControlHeight = _refreshControl.Frame.Size.Height;

				return true;
			}

			if (view is UICollectionView)
			{
				var collectionView = view as UICollectionView;

				if (CanUseRefreshControlProperty())
					collectionView.RefreshControl = _refreshControl;
				else
					collectionView.InsertSubview(_refreshControl, index);

				_origininalY = collectionView.ContentOffset.Y;
				_refreshControlHeight = _refreshControl.Frame.Size.Height;

				return true;
			}

			if (view is UIWebView)
			{
				var uiWebView = view as UIWebView;
				uiWebView.ScrollView.InsertSubview(_refreshControl, index);
				return true;
			}

			if (view is UIScrollView)
			{
				var scrollView = view as UIScrollView;

				if (CanUseRefreshControlProperty())
					scrollView.RefreshControl = _refreshControl;
				else
					scrollView.InsertSubview(_refreshControl, index);

				scrollView.AlwaysBounceVertical = true;

				_origininalY = scrollView.ContentOffset.Y;
				_refreshControlHeight = _refreshControl.Frame.Size.Height;

				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryInsertRefresh(control, i))
					return true;
			}

			return false;
		}

		void UpdateColors()
		{
			if (Element == null || _refreshControl == null)
				return;

			if (Element.RefreshColor != Color.Default)
				_refreshControl.TintColor = Element.RefreshColor.ToUIColor();

			SetBackgroundColor(Element.BackgroundColor);
		}

		void UpdateIsRefreshing()
		{
			IsRefreshing = Element.IsRefreshing;
		}

		void UpdateIsEnabled()
		{
			if (Element.IsEnabled)
				TryInsertRefresh(_refreshControlParent);
			else
			{
				if (_refreshControl.Superview != null)
					_refreshControl.RemoveFromSuperview();
			}
		}

		bool CanUseRefreshControlProperty()
		{
			return Forms.IsiOS10OrNewer && !_usingLargeTitles;
		}

		void OnRefresh(object sender, EventArgs e)
		{
			if (Element?.Command?.CanExecute(Element?.CommandParameter) ?? false)
			{
				Element.Command.Execute(Element?.CommandParameter);
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, this, NativeView);
		}
	}
}