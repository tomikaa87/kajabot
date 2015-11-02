using System;

namespace KajaBot
{
    internal interface IRestaurant : IEquatable<IRestaurant>
    {
        string GetName();
        string GetCommand();
        string GetCurrentMenu();
    }
}