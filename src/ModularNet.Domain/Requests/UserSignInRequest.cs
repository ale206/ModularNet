namespace ModularNet.Domain.Requests;

public class UserSignInRequest
{
    public Guid Id { get; set; }
    public DateTime? AuthenticatedOn { get; set; }
}