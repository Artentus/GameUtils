namespace Artentus.GameUtils.Network
{
    public interface IPackageTransmitter
    {
        void Send<T>(T package) where T : IPackage;

        void Send<T>(T package, IConnection[] connections) where T : IPackage;
    }
}
