using System.Globalization;

namespace Poshteh.Core.ApplicationService.Localization;

public sealed class MessageLocalizationOptions
{
    public string DefaultCultureName { get; set; } = CultureInfo.InvariantCulture.Name;

    public IList<string> SupportedCultureNames { get; } = new List<string>();

    public string FallbackKey { get; set; } = "Common.UnexpectedError";

    public string FallbackMessage { get; set; } = "An unexpected error occurred.";

    public string UnexpectedErrorCode { get; set; } = "Common.UnexpectedError";

    public CultureInfo GetDefaultCulture()
    {
        try
        {
            return CultureInfo.GetCultureInfo(DefaultCultureName);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}
