using UnityEngine;
using UnityEngine.UI;

namespace IdlePlus.Utilities.Extensions {
	public static class UnityUIExtension {
		
		// ContentSizeFitter
		
		public static ContentSizeFitter SetFit(this ContentSizeFitter fitter, ContentSizeFitter.FitMode fit) {
			fitter.horizontalFit = fit;
			fitter.verticalFit = fit;
			return fitter;
		}
        
		public static ContentSizeFitter SetFit(this ContentSizeFitter fitter, ContentSizeFitter.FitMode horizontalFit, 
			ContentSizeFitter.FitMode verticalFit) {
			fitter.horizontalFit = horizontalFit;
			fitter.verticalFit = verticalFit;
			return fitter;
		}
		
		public static ContentSizeFitter SetHorizontalFit(this ContentSizeFitter fitter,
			ContentSizeFitter.FitMode horizontalFit) {
			fitter.horizontalFit = horizontalFit;
			return fitter;
		}
		
		public static ContentSizeFitter SetVerticalFit(this ContentSizeFitter fitter,
			ContentSizeFitter.FitMode verticalFit) {
			fitter.verticalFit = verticalFit;
			return fitter;
		}
		
		// HorizontalOrVerticalLayoutGroup

		public static HorizontalOrVerticalLayoutGroup DisableChildStates(this HorizontalOrVerticalLayoutGroup group) {
			group.childControlHeight = false;
			group.childControlWidth = false;
			group.childForceExpandHeight = false;
			group.childForceExpandWidth = false;
			group.childScaleHeight = false;
			group.childScaleWidth = false;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetChildControl(this HorizontalOrVerticalLayoutGroup group, 
			bool controlWidth, bool controlHeight) {
			group.childControlWidth = controlWidth;
			group.childControlHeight = controlHeight;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetChildForceExpand(this HorizontalOrVerticalLayoutGroup group, 
			bool forceExpandWidth, bool forceExpandHeight) {
			group.childForceExpandWidth = forceExpandWidth;
			group.childForceExpandHeight = forceExpandHeight;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetChildScale(this HorizontalOrVerticalLayoutGroup group, 
			bool scaleWidth, bool scaleHeight) {
			group.childScaleWidth = scaleWidth;
			group.childScaleHeight = scaleHeight;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetChildStates(this HorizontalOrVerticalLayoutGroup group, 
			bool controlWidth = false, bool controlHeight = false, bool forceExpandWidth = false, 
			bool forceExpandHeight = false, bool scaleWidth = false, bool scaleHeight = false) {
			group.childControlWidth = controlWidth;
			group.childControlHeight = controlHeight;
			group.childForceExpandWidth = forceExpandWidth;
			group.childForceExpandHeight = forceExpandHeight;
			group.childScaleWidth = scaleWidth;
			group.childScaleHeight = scaleHeight;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetSpacing(this HorizontalOrVerticalLayoutGroup group,
			float spacing) {
			group.spacing = spacing;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetPadding(this HorizontalOrVerticalLayoutGroup group,
			RectOffset padding) {
			group.padding = padding;
			return group;
		}
		
		public static HorizontalOrVerticalLayoutGroup SetPadding(this HorizontalOrVerticalLayoutGroup group,
			int left, int right, int top, int bottom) {
			group.padding = new RectOffset(left, right, top, bottom);
			return group;
		}
	}
}