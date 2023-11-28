using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.ValueObjects.EntityHelper;

[Table("workers_services")]
internal class WorkerServiceRelation
{
    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    [Column("service_id")]
    public Guid ServiceId { get; set; }

    public WorkerServiceRelation(Guid workerId, Guid serviceId)
    {
        WorkerId = workerId;
        ServiceId = serviceId;
    }
}
