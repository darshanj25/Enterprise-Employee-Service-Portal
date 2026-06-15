import { IInputs, IOutputs } from "./generated/ManifestTypes";

export class EmployeeContextCard implements ComponentFramework.StandardControl<IInputs, IOutputs> {

    private container: HTMLDivElement;
    private context: ComponentFramework.Context<IInputs>;

    constructor() {
        // Empty
    }

    public init(
        context: ComponentFramework.Context<IInputs>,
        notifyOutputChanged: () => void,
        state: ComponentFramework.Dictionary,
        container: HTMLDivElement
    ): void {

        this.context = context;
        this.container = container;

        this.renderLoading();
    }

    public async updateView(
        context: ComponentFramework.Context<IInputs>
    ): Promise<void> {

        this.context = context;

        try {

            const recordId =
                ((context.mode as { contextInfo?: { entityId?: string } }).contextInfo)?.entityId;

            if (!recordId) {

                this.container.innerHTML = `
                    <div style="padding:10px;">
                        Save the record to view employee context.
                    </div>
                `;

                return;
            }

            const leaveRequest =
                await context.webAPI.retrieveRecord(
                    "djs_leaverequest",
                    recordId,
                    "?$select=djs_name,djs_requesteddays,djs_leavebalanceatsubmission,_djs_employee_value"
                );

            const employeeId =
                leaveRequest["_djs_employee_value"];

            if (!employeeId) {

                this.container.innerHTML = `
        <div style="padding:10px;">
            No employee selected.
        </div>
    `;

                return;
            }

            const employee =
    await context.webAPI.retrieveRecord(
        "djs_employee",
        employeeId,
        "?$select=djs_name,djs_email,djs_leavebalance,djs_employmentstatus,_djs_department_value,_djs_manager_value"
    );

            const leaveBalance =
                employee["djs_leavebalance"] ?? 0;

            const requestedDays =
                leaveRequest["djs_requesteddays"] ?? 0;

            const balanceAfterApproval =
                leaveBalance - requestedDays;

            const department =
                employee["_djs_department_value@OData.Community.Display.V1.FormattedValue"] ?? "N/A";

            const manager =
                employee["_djs_manager_value@OData.Community.Display.V1.FormattedValue"] ?? "N/A";

            const email =
                employee["djs_email"] ?? "N/A";

            const employeeName =
                employee["djs_name"] ?? "N/A";

            const employmentStatus =
                employee["djs_employmentstatus@OData.Community.Display.V1.FormattedValue"] ?? "N/A";

            let status = "🟢 Healthy";

            if (balanceAfterApproval < 5) {
                status = "🔴 Critical";
            }
            else if (balanceAfterApproval <= 10) {
                status = "🟡 Warning";
            }

            this.container.innerHTML = `
<div style="
    padding:16px;
    border:1px solid #d9d9d9;
    border-radius:8px;
    background-color:#fafafa;
    font-family:Segoe UI;
">

<h3>Employee Context</h3>

<div style="margin-bottom:12px;">
    <h4>Employee Details</h4>

    <p><b>Name:</b> ${employeeName}</p>
    <p><b>Department:</b> ${department}</p>
    <p><b>Manager:</b> ${manager}</p>
    <p><b>Email:</b> ${email}</p>
    <p><b>Employment Status:</b> ${employmentStatus}</p>
</div>

<div>
    <h4>Leave Analysis</h4>

    <p><b>Current Leave Balance:</b> ${leaveBalance}</p>
    <p><b>Requested Days:</b> ${requestedDays}</p>
    <p><b>Balance After Approval:</b> ${balanceAfterApproval}</p>
    <p><b>Status:</b> ${status}</p>
</div>

</div>
`;


        } catch (error) {

            this.container.innerHTML = `
                <div style="color:red;">
                    ${(error as Error).message}
                </div>
            `;
        }
    }

    public getOutputs(): IOutputs {
        return {};
    }

    public destroy(): void {
        // Add code to cleanup control if necessary
    }

    private renderLoading(): void {

        this.container.innerHTML = `
            <div style="padding:10px;">
                Loading...
            </div>
        `;
    }
}
