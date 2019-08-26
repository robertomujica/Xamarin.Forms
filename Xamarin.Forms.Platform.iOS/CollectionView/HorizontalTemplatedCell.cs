using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class HorizontalTemplatedCell : TemplatedCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.HorizontalTemplatedCell");

		[Export("initWithFrame:")]
		public HorizontalTemplatedCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = VisualElementRenderer.Element.Measure(double.PositiveInfinity, 
				ConstrainedDimension, MeasureFlags.IncludeMargins);

			return new CGSize(measure.Request.Width, ConstrainedDimension);
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ConstrainedDimension = constraint.Height;
			Layout(constraint);
		}

		protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		{
			var size = Size.Zero;

			if (VisualElementRenderer?.Element == null)
			{
				return (false, size);
			}

			var bounds = VisualElementRenderer.Element.Bounds;

			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return (false, size);
			}

			var desiredBounds = VisualElementRenderer.Element.Measure(double.PositiveInfinity, bounds.Height, 
				MeasureFlags.IncludeMargins);

			if (desiredBounds.Request.Width == currentSize.Width)
			{
				// Nothing in the cell needs more room, so leave it as it is
				return (false, size);
			}

			return (true, desiredBounds.Request);
		}
	}
}