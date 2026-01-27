using Shared.Network.Messages;

namespace Shared.Network;

public class Login : ISerializable<Login>, IRequestable
{
    public string Name;

    public static RequestType RequestType => RequestType.Login;

    public Login(string name)
    {
        Name = name;
    }
    public void Encode(BinaryWriter writer)
    {
        writer.Write(Name);
    }

    public static Login Decode(BinaryReader reader)
    {
        return new Login(
            reader.ReadString()
        );
    }
}