
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;


using MechanicShop.Domain.Workorders.Billing;
using MechanicShop.Domain.Workorders.Enums;

namespace MechanicShop.Domain.Workorders;

public sealed class WorkOrder : AuditableEntity
{

    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }

    public WorkOrderState State { get; private set; }


    public Invoice? Invoice { get; set; }
    public decimal? Discount { get; private set; }
    public decimal? Tax { get; private set; }
    public decimal? TotalPartsCost => 0;
    public decimal? TotalLaborCost => 0;
    public decimal? Total => (TotalPartsCost ?? 0) + (TotalLaborCost ?? 0);

   


    private WorkOrder()
    { }

    private WorkOrder(Guid id, DateTimeOffset startAt, DateTimeOffset endAt , WorkOrderState state )
        : base(id)
    {
      
        StartAtUtc = startAt;
        EndAtUtc = endAt;
        State = state;
       
    }

    public static Result<WorkOrder> Create(Guid id, DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (id == Guid.Empty)
        {
            return WorkOrderErrors.WorkOrderIdRequired;
        }

      

        if (endAt <= startAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

       

        return new WorkOrder(id, startAt, endAt, WorkOrderState.Scheduled);
    }

  

    public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.TimingReadonly(Id.ToString(), State);
        }

        if (endAt <= startAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        StartAtUtc = startAt;
        EndAtUtc = endAt;

        return Result.Updated;
    }

   

    public Result<Updated> UpdateState(WorkOrderState newState)
    {
        if (!CanTransitionTo(newState))
        {
            return WorkOrderErrors.InvalidStateTransition(State, newState);
        }

        State = newState;

        return Result.Updated;
    }

    public bool IsEditable => State is not (WorkOrderState.Completed or WorkOrderState.Cancelled or WorkOrderState.InProgress);

    public bool CanTransitionTo(WorkOrderState newStatus)
    {
        return (State, newStatus) switch
        {
            (WorkOrderState.Scheduled, WorkOrderState.InProgress) => true,
            (WorkOrderState.InProgress, WorkOrderState.Completed) => true,
            (_, WorkOrderState.Cancelled) when State != WorkOrderState.Completed => true,
            _ => false
        };
    }

    public Result<Updated> Cancel()
    {
        if (!CanTransitionTo(WorkOrderState.Cancelled))
        {
            return WorkOrderErrors.InvalidStateTransition(State, WorkOrderState.Cancelled);
        }

        State = WorkOrderState.Cancelled;
        return Result.Updated;
    }
}