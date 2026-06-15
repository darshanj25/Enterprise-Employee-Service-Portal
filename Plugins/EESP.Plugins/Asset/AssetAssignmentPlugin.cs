using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace EESP.Plugins
{
    public class AssetAssignmentPlugin : PluginBase
    {
        public AssetAssignmentPlugin(
            string unsecureConfiguration,
            string secureConfiguration)
            : base(typeof(AssetAssignmentPlugin))
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

            if (target.LogicalName != "djs_assetassignment")
            {
                return;
            }

            if (!target.Contains("djs_asset"))
            {
                throw new InvalidPluginExecutionException(
                    "Asset is required.");
            }

            EntityReference assetRef =
                target.GetAttributeValue<EntityReference>("djs_asset");

            var service = localPluginContext.PluginUserService;

            Entity asset = service.Retrieve(
                "djs_asset",
                assetRef.Id,
                new ColumnSet("djs_status"));

            int assetStatus =
                asset.GetAttributeValue<OptionSetValue>("djs_status").Value;

            // Assigned = 1
            if (assetStatus == 1)
            {
                throw new InvalidPluginExecutionException(
                    "This asset is already assigned.");
            }

            // Update Asset Status
            Entity assetToUpdate =
                new Entity("djs_asset", assetRef.Id);

            assetToUpdate["djs_status"] =
                new OptionSetValue(1);

            service.Update(assetToUpdate);

            localPluginContext.Trace(
                "Asset status updated successfully.");

            // Create Approval History
            Entity approvalHistory =
                new Entity("djs_approvalhistory");

            approvalHistory["djs_action"] =
    new OptionSetValue(0);

            approvalHistory["djs_actiondate"] =
                DateTime.UtcNow;

            // approvalHistory["djs_approver"] =
            //     new EntityReference(
            //         "systemuser",
            //         context.InitiatingUserId);

            service.Create(approvalHistory);

            localPluginContext.Trace(
                "Approval history created successfully.");
        }
    }
}