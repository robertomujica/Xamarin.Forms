using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class VerticalTemplatedCell : TemplatedCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.VerticalTemplatedCell");

		[Export("initWithFrame:")]
		public VerticalTemplatedCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = VisualElementRenderer.Element.Measure(ConstrainedDimension, 
				double.PositiveInfinity, MeasureFlags.IncludeMargins);

			return new CGSize(ConstrainedDimension, measure.Request.Height);
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ConstrainedDimension = constraint.Width;
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

			var desiredBounds = VisualElementRenderer.Element.Measure(bounds.Width, double.PositiveInfinity, 
				MeasureFlags.IncludeMargins);

			if (desiredBounds.Request.Height == currentSize.Height)
			{
				// Nothing in the cell needs more room, so leave it as it is
				return (false, size);
			}

			return (true, desiredBounds.Request);
		}
	}
}