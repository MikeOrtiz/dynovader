<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="MyScienceOnCloud" generation="1" functional="0" release="0" Id="14ec9df7-fed8-456f-a5d8-fcf3ff8e15f9" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="MyScienceOnCloudGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="MyScienceServiceWebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/LB:MyScienceServiceWebRole:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="MyScienceServiceWebRole:?IsSimulationEnvironment?" defaultValue="">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRole:?IsSimulationEnvironment?" />
          </maps>
        </aCS>
        <aCS name="MyScienceServiceWebRole:?RoleHostDebugger?" defaultValue="">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRole:?RoleHostDebugger?" />
          </maps>
        </aCS>
        <aCS name="MyScienceServiceWebRole:?StartupTaskDebugger?" defaultValue="">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRole:?StartupTaskDebugger?" />
          </maps>
        </aCS>
        <aCS name="MyScienceServiceWebRole:BlobConnection" defaultValue="">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRole:BlobConnection" />
          </maps>
        </aCS>
        <aCS name="MyScienceServiceWebRole:ContainerName" defaultValue="">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRole:ContainerName" />
          </maps>
        </aCS>
        <aCS name="MyScienceServiceWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="MyScienceServiceWebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MapMyScienceServiceWebRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:MyScienceServiceWebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapMyScienceServiceWebRole:?IsSimulationEnvironment?" kind="Identity">
          <setting>
            <aCSMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/?IsSimulationEnvironment?" />
          </setting>
        </map>
        <map name="MapMyScienceServiceWebRole:?RoleHostDebugger?" kind="Identity">
          <setting>
            <aCSMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/?RoleHostDebugger?" />
          </setting>
        </map>
        <map name="MapMyScienceServiceWebRole:?StartupTaskDebugger?" kind="Identity">
          <setting>
            <aCSMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/?StartupTaskDebugger?" />
          </setting>
        </map>
        <map name="MapMyScienceServiceWebRole:BlobConnection" kind="Identity">
          <setting>
            <aCSMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/BlobConnection" />
          </setting>
        </map>
        <map name="MapMyScienceServiceWebRole:ContainerName" kind="Identity">
          <setting>
            <aCSMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/ContainerName" />
          </setting>
        </map>
        <map name="MapMyScienceServiceWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapMyScienceServiceWebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="MyScienceServiceWebRole" generation="1" functional="0" release="0" software="C:\Users\Lu Li\Documents\dynovader\MyScienceOnCloud\MyScienceOnCloud\bin\Debug\MyScienceOnCloud.csx\roles\MyScienceServiceWebRole" entryPoint="base\x86\WaHostBootstrapper.exe" parameters="base\x86\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="?IsSimulationEnvironment?" defaultValue="" />
              <aCS name="?RoleHostDebugger?" defaultValue="" />
              <aCS name="?StartupTaskDebugger?" defaultValue="" />
              <aCS name="BlobConnection" defaultValue="" />
              <aCS name="ContainerName" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;MyScienceServiceWebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;MyScienceServiceWebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="MyScienceServiceWebRole.svclog" defaultAmount="[1000,1000,1000]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRoleInstances" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyID name="MyScienceServiceWebRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="edc92305-0be4-4ef6-bc0c-ea42fd83233a" ref="Microsoft.RedDog.Contract\ServiceContract\MyScienceOnCloudContract@ServiceDefinition.build">
      <interfacereferences>
        <interfaceReference Id="cdcdfce9-9de5-4ef4-a451-71c2dfd94997" ref="Microsoft.RedDog.Contract\Interface\MyScienceServiceWebRole:Endpoint1@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/MyScienceOnCloud/MyScienceOnCloudGroup/MyScienceServiceWebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>