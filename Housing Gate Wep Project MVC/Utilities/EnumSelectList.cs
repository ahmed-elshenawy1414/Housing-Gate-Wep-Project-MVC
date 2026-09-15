using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace StudentHousing.Helpers
{
    /// <summary>
    /// Localized drop-down items for enums. Mirror of Html.GetEnumSelectList but the
    /// option text is read from resources via EnumDisplay so it follows the current
    /// culture. Values stay numeric so model binding and the select tag helper
    /// behave exactly like the built-in list.
    /// </summary>
    public static class EnumSelectList
    {
        public static List<SelectListItem> Get<TEnum>(IStringLocalizer? localizer = null) where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(value => new SelectListItem
                {
                    Value = Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
                    Text = EnumDisplay.Get(value, localizer)
                })
                .ToList();
        }
    }
}
