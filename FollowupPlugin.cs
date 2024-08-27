using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using System.Net.Http;
using Newtonsoft.Json;



namespace ContactsPlugin
{
    public class FollowupPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService)); 
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.MessageName == "Retrieve" || context.MessageName == "RetrieveMultiple")
            {
                Entity entity = context.OutputParameters["BusinessEntity"] as Entity;
                try {

                    if (entity != null && entity.LogicalName == "contact")
                    {
                        QueryExpression query = new QueryExpression("new_transaction")
                        {
                            ColumnSet = new ColumnSet("new_transactionid", "new_status")
                        };

                        FilterExpression orFilter = new FilterExpression(LogicalOperator.Or);
                        orFilter.AddCondition("new_status", ConditionOperator.Equal, 1);
                        orFilter.AddCondition("new_status", ConditionOperator.Equal, 2);
                        orFilter.AddCondition("new_status", ConditionOperator.Equal, 3);
                        orFilter.AddCondition("new_status", ConditionOperator.Equal, 6);
                        orFilter.AddCondition("new_status", ConditionOperator.Equal, 7);

                        FilterExpression mainFilter = new FilterExpression(LogicalOperator.And);
                        mainFilter.AddFilter(orFilter);
                        mainFilter.AddCondition("new_client", ConditionOperator.Equal, entity.Id);

                        query.Criteria = mainFilter;
                        IOrganizationService service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(context.UserId);
                        EntityCollection result = service.RetrieveMultiple(query);

                        if (result.Entities.Count > 0)
                        {
                            entity.Attributes["new_activetransactions"] = "true";
                        }
                    }
                }
                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }
            }
        }
        
    }

}


