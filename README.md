## Install

Via [nuget](https://www.nuget.org/packages/OperationLogger.Wcf).

## Usage
After adding a reference to the OperationLogger.Wcf.dll, logging can be enabled with one simple code change and a bit of configuration.

### Wiring up logging function
On service initialization (for example in custom ServiceHost's `OnOpening` method) setup the `OperationLogBehavior.LogAction` static function, this will be called with `OperationDetails` whenever an operation is about to execute:

```csharp
OperationLogBehavior.LogAction = operationDetails => { /*TODO: log to db/file/whatever*/ }
```

### Configuration
First enable the use of the service behavior using the behavior extension element:

```
<system.serviceModel>
    <extensions>
        <behaviorExtensions>
            <add name="operationLog" type="OperationLogger.Wcf.OperationLogBehaviorElement, OperationLogger.Wcf" />
        </behaviorExtensions>
        ...
```

Then add this behavior to the service's behavior definition:

```
<behaviors>
    <serviceBehaviors>
        <behavior name="MyServiceBehavior">
          
            <operationLog>
                <patterns>
                    <add actionPattern=".*" />
                </patterns>
            </operationLog>
            ...
```

The above configuration example enables logging for all actions. To restrict logging to specific services/actions, add as many `pattern` entries as needed with the `actionPattern` set to a regular expression that needs to be matched for logging to be applied. For example:

```
<operationLog>
    <patterns>
        <add actionPattern=".*IPatientService/FindPatients" />
        <add actionPattern=".*IPatientService/GetPatient" />
    </patterns>
</operationLog>
```

Parameter logging can be controlled similarly, using the `parameterPattern` attribute. By default it is turned on for all parameters, but it can be turned off altogether:

```
<operationLog>
    <patterns>
        <add actionPattern=".*IAuthenticationService/Authenticate" parameterPattern="^$" />
    </patterns>
</operationLog>
```

Or it could be disabled for specific parameters:

```
<operationLog>
    <patterns>
        <add actionPattern=".*IAuthenticationService/Authenticate" parameterPattern="^((?!password).)*$" />
    </patterns>
</operationLog>
```

