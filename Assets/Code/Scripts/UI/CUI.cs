using UnityEngine.UIElements;

/// <summary>
/// CUI (Ceterra UI) provides utility methods for manipulating Unity UI VisualElements.
/// These helper functions simplify showing, hiding, toggling, and checking visibility of UI elements.
/// </summary>
/// <remarks>
/// This utility class contains static methods only and does not need to be instantiated.
/// </remarks>
public class CUI {
    /// <summary>
    /// Checks if a VisualElement is currently hidden.
    /// </summary>
    /// <param name="element">The VisualElement to check</param>
    /// <returns>True if the element has DisplayStyle.None; otherwise, false</returns>
    public static bool IsHidden(VisualElement element) {
        return element.resolvedStyle.display == DisplayStyle.None;
    }

    /// <summary>
    /// Makes a VisualElement visible by setting its display style to Flex.
    /// Only changes the display style if the element is currently hidden.
    /// </summary>
    /// <param name="element">The VisualElement to show</param>
    public static void Show(VisualElement element) {
        if (IsHidden(element)) {
            element.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Hides a VisualElement by setting its display style to None.
    /// Only changes the display style if the element is currently visible.
    /// </summary>
    /// <param name="element">The VisualElement to hide</param>
    public static void Hide(VisualElement element) {
        if (!IsHidden(element)) {
            element.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Toggles the visibility state of a VisualElement.
    /// If the element is hidden, it will be shown; if it's visible, it will be hidden.
    /// </summary>
    /// <param name="element">The VisualElement to toggle</param>
    public static void Toggle(VisualElement element) {
        if (IsHidden(element)) {
            Show(element);
        }
        else {
            Hide(element);
        }
    }
}