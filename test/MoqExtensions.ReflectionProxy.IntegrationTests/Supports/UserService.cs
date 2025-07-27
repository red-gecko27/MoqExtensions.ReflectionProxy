namespace MoqExtensions.ReflectionProxy.IntegrationTests.Supports;

public interface IUserService
{
    User GetUserById(int id);
    User CreateUser(CreateUserRequest request);
}

public class UserService : IUserService
{
    private static int _userNextId = 1;

    public User GetUserById(int id)
    {
        return new User { Id = id, Name = $"User {id}" };
    }

    public User CreateUser(CreateUserRequest request)
    {
        return new User { Id = _userNextId++, Name = request.Name };
    }
}

#region Model Dependencies

public class User : IComparable<User>
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    public int CompareTo(User? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var idComparison = Id.CompareTo(other.Id);
        if (idComparison != 0) return idComparison;
        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
}

public class CreateUserRequest
{
    public required string Name { get; set; }
}

#endregion