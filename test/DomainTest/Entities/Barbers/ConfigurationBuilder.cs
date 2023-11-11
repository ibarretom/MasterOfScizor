using Bogus;
using Domain.Entities.Barbers;
using Domain.ValueObjects.Enums;

namespace DomainTest.Entities.Barbers;

internal class ConfigurationBuilder
{
    public static Configuration BuildRandom()
    {
        var faker = new Faker();

        return new Configuration(faker.Random.Enum<OrderQueueType>(), faker.Random.Bool(), 
                faker.Random.Bool() ? faker.Random.Int(1, 15) : null, faker.Date.Timespan(), 
                faker.Random.Bool());
    }

    public static Configuration BuildQueueWithVirtualQueue()
    {
        return new Configuration(OrderQueueType.Queue, true, null, TimeSpan.Zero, false);
    }

    public static Configuration BuildWithQueueLimit(int queueLimit = 9)
    {
        return new Configuration(OrderQueueType.Queue, false, queueLimit, TimeSpan.Zero, false);
    }

    public static Configuration BuildWithScheduleWithNoDelay(int scheduleDefaultInterval = 30)
    {
        var configuration = new Configuration(OrderQueueType.Schedule, false, null, TimeSpan.Zero, false)
        {
            ScheduleDefaultInterval = TimeSpan.FromMinutes(scheduleDefaultInterval)
        };

        return configuration;
    }

    public static Configuration BuildWithScheduleWithDelay(int delay = 20, int defaultInterval = 30)
    {
        return new Configuration(OrderQueueType.Schedule, false, null, TimeSpan.FromMinutes(delay), false)
        {
            ScheduleDefaultInterval = TimeSpan.FromMinutes(defaultInterval)
        };
    }
}
