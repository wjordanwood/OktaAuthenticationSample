namespace OktaWebApp.Models;

public class CurrentUserViewModel
{
    public List<Claim> Claims { get; set; }
}

public class Claim
{
    public string Type { get; set; }
    public string Value { get; set; }
}