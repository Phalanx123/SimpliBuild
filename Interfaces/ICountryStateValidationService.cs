using simpliBuild.SWMS.Model;

namespace simpliBuild.Interfaces;

public interface ICountryStateValidationService
{
    bool IsStateOrRegionValidForCountry(StateOrRegion state, Country country);
}