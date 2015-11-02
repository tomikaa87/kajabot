using System.Linq;
using System.Text;

namespace KajaBot
{
    internal class BotCommandListRestaurants : IBotCommand
    {
        private readonly RestaurantManager _restaurantManager;

        public BotCommandListRestaurants(RestaurantManager restaurantManager)
        {
            _restaurantManager = restaurantManager;
        }

        public string GetCommand()
        {
            return "lista";
        }

        public string RunAction(string[] args)
        {
            IRestaurant[] restaurants = _restaurantManager.GetRestaurants();

            if (!restaurants.Any())
                return "Nincs általam ismert étterem. :(";

            var sb = new StringBuilder();
            sb.AppendLine("Az általam ismert éttermek:");

            foreach (IRestaurant restaurant in restaurants)
            {
                sb.Append("`");
                sb.Append(restaurant.GetCommand());
                sb.Append("`: ");
                sb.AppendLine(restaurant.GetName());
            }

            return sb.ToString();
        }

        public bool Equals(IBotCommand other)
        {
            if (other == null)
                return false;

            return other.GetCommand() == GetCommand();
        }
    }
}