using simpliBuild.Interfaces;
using simpliBuild.SWMS.Model;

namespace simpliBuild.Services;

public class CountryStateRegionValidationService : ICountryStateValidationService
{
    public bool IsStateOrRegionValidForCountry(StateOrRegion state, Country country)
    {
        return country switch
        {
            Country.Australia => (int)state < 10,
            Country.NewZealand => (int)state > 10,
            _ => false
        };
    }
}