using System;
using System.Collections.Generic;
using log4net;

namespace KajaBot
{
    internal class RestaurantManager
    {
        private readonly ILog _log = LogManager.GetLogger("RestaurantManager");
        private readonly List<IRestaurant> _restaurants = new List<IRestaurant>();

        /// <summary>
        ///     Adds a new restaurant
        /// </summary>
        /// <param name="restaurant"></param>
        public void AddRestaurant(IRestaurant restaurant)
        {
            if (_restaurants.Contains(restaurant))
                throw new Exception("Restaurant is already added");

            _restaurants.Add(restaurant);

            _log.Info("Restaurant added: " + restaurant.GetCommand() + " (" + restaurant.GetName() + ")");
        }

        /// <summary>
        ///     Returns a list of known restaurants
        /// </summary>
        /// <returns></returns>
        public IRestaurant[] GetRestaurants()
        {
            return _restaurants.ToArray();
        }

        /// <summary>
        ///     Checks if the manager has a restaurant for the given command.
        /// </summary>
        /// <param name="restaurantCommand"></param>
        /// <returns></returns>
        public bool ContainsRestaurant(string restaurantCommand)
        {
            IRestaurant restaurant = _restaurants.Find(x => x.GetCommand().Equals(restaurantCommand));
            return restaurant != null;
        }

        /// <summary>
        ///     Returns the current menu of the given restaurant.
        /// </summary>
        /// <param name="restaurantCommand"></param>
        /// <returns></returns>
        public string GetMenu(string restaurantCommand)
        {
            IRestaurant restaurant = _restaurants.Find(x => x.GetCommand().Equals(restaurantCommand));
            if (restaurant == null)
                throw new Exception("Unknown restaurant: " + restaurantCommand.Substring(0, 100));

            StatisticsCollector.GetInstance().IncrementExecutedRestaurantMenuRequestCount();

            _log.Info("Getting menu of restaurant: " + restaurant.GetName());

            return restaurant.GetCurrentMenu();
        }
    }
}