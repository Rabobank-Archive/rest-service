[![build](https://github.com/azure-devops-compliance/rest-service/workflows/nuget/badge.svg)](https://github.com/azure-devops-compliance/rest-service/actions)
[![codecov](https://codecov.io/gh/azure-devops-compliance/rest-service/branch/master/graph/badge.svg)](https://codecov.io/gh/azure-devops-compliance/rest-service)

# Azure DevOps Compliance - Rest Service

Instead of using the official SDK we hand rolled a REST client with method to build request and data types to (de)serialize the transferred state.

## netcore

In mid-18, when this journey started, the SDK was still not available for dotnet core.

## Testable

For me, the SDK is notorious for it's lack of testability. No interfaces are used and constructors expect live connections that do actual calls.

## Injectable

The lack of interfaces makes it nearly impossible to stub requests to the point where you need to return representative JSON documents or even
have a live connection to Azure DevOps.   

## Undocumented API calls

Though the REST API is pretty well documented it turns out the official fronted uses a handful of 'REST' calls that are not official available. Simple
tasks like retrieving all groups and the members of that groups is a real pain with the graph API (not sure if it is even possible).

## Underdocumented API calls

Even when using the official SDK you have to deal with low level details of the API endpoints in a way the SDK is not really helping. You need
to know how to construct a group name with some magic name for [example](https://github.com/microsoft/azure-devops-dotnet-samples/blob/master/ServiceHooks/Utilities/Permissions/RestoreManagePermissionsToProjectAdminGroups.cs#L39).
So its even easier to reverse engineer the `XHRs` that the frontend makes when altering permissions.

## Usage

The working of this client is documented [here](https://medium.com/@MRiezebosch/building-a-testable-and-extensible-rest-client-as-an-abstraction-6ffa48b3f3ab).