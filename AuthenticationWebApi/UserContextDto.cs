namespace AuthenticationWebApi;
public record UserContextDto(
    int company_id, 
    int location_id, 
    string role, 
    List<string> permissions);