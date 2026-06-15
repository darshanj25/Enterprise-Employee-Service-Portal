using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace EESP.Plugins
{
    public class LeaveRequestPlugin : PluginBase
    {
        public LeaveRequestPlugin(
            string unsecureConfiguration,
            string secureConfiguration)
            : base(typeof(LeaveRequestPlugin))
        {
        }

        protected override void ExecuteDataversePlugin(
            ILocalPluginContext localPluginContext)
        {
            
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            if (target.LogicalName != "djs_leaverequest")
            {
                return;
            }

            if (!target.Contains("djs_startdate") ||
                !target.Contains("djs_enddate") ||
                !target.Contains("djs_employee"))
            {
                return;
            }

            DateTime startDate =
                target.GetAttributeValue<DateTime>("djs_startdate");

            DateTime endDate =
                target.GetAttributeValue<DateTime>("djs_enddate");

            if (endDate < startDate)
            {
                throw new InvalidPluginExecutionException(
                    "End Date cannot be earlier than Start Date.");
            }

            int requestedDays =
                (endDate.Date - startDate.Date).Days + 1;

            target["djs_requesteddays"] = requestedDays;

            EntityReference employeeRef =
                target.GetAttributeValue<EntityReference>("djs_employee");

            var service = localPluginContext.PluginUserService;

            Entity employee = service.Retrieve(
                "djs_employee",
                employeeRef.Id,
                new ColumnSet("djs_leavebalance"));

            int availableBalance =
                employee.GetAttributeValue<int>("djs_leavebalance");

            target["djs_leavebalanceatsubmission"] =
                availableBalance;

            if (requestedDays > availableBalance)
            {
                throw new InvalidPluginExecutionException(
                    $"Insufficient leave balance. Available: {availableBalance}, Requested: {requestedDays}");
            }

            localPluginContext.Trace(
                $"Leave Request Validation Successful. Requested Days: {requestedDays}, Available Balance: {availableBalance}");
        }
    }
}