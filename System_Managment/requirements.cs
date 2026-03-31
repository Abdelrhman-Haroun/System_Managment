using System.Collections.Generic;

namespace System_Managment;

public static class Requirements
{
    public static IReadOnlyList<RequirementItem> All { get; } =
        new List<RequirementItem>
        {
            new(
                "Filters layout",
                "Make any filtration appear at the top of the page instead of under summary boxes across the app."),
            new(
                "User registration",
                "Fix the register button for users and make validation messages visible to the user."),
            new(
                "SweetAlert consistency",
                "Fix missing SweetAlert styles and unify the alert appearance across the app."),
            new(
                "Image cleanup",
                "When an uploaded image is deleted or replaced, remove the old file to save server space."),
            new(
                "Aside menu width",
                "Reduce the aside menu width and make it fit its elements better to save content space and improve usability."),
            new(
                "Internal usage by product",
                "If the feature is unused, remove it. If it is used, make it meaningful and fix its styling."),
            new(
                "Invoice printing",
                "Fix invoice printing issues so the print date does not overlap the logo, output only one client copy, and ensure the layout is ready for A4 printing."),
            new(
                "General UX",
                "Improve the overall user experience, modernize the design, and make the app easier to use and navigate.")
        };
}

public sealed record RequirementItem(string Title, string Description);
