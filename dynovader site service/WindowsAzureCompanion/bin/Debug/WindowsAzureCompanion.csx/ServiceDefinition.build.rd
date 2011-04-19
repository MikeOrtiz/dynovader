<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="WindowsAzureCompanion" generation="1" functional="0" release="0" Id="d37b4dbf-a7fb-441c-8628-c72693b0203c" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="WindowsAzureCompanionGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="AdminWebSite:HttpIn" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/LB:AdminWebSite:HttpIn" />
          </inToChannel>
        </inPort>
        <inPort name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/LB:AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
        <inPort name="AdminWebSite:VMManagerServiceExternalHttpPort" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/LB:AdminWebSite:VMManagerServiceExternalHttpPort" />
          </inToChannel>
        </inPort>
        <inPort name="AdminWebSite:WindowsAzureCompanionHttpIn" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/LB:AdminWebSite:WindowsAzureCompanionHttpIn" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="AdminWebSite:?IsSimulationEnvironment?" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:?IsSimulationEnvironment?" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:?RoleHostDebugger?" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:?RoleHostDebugger?" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:?StartupTaskDebugger?" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:?StartupTaskDebugger?" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:AdminPassword" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:AdminPassword" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:AdminUserName" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:AdminUserName" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:ApplicationDescription" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:ApplicationDescription" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:ApplicationTitle" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:ApplicationTitle" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:InstallationStatusConfigFileBlob" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:InstallationStatusConfigFileBlob" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:PHPApplicationsBackupContainerName" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:PHPApplicationsBackupContainerName" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:ProductListXmlFeed" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:ProductListXmlFeed" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:ProgressInformationFileBlob" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:ProgressInformationFileBlob" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:WindowsAzureStorageAccountKey" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:WindowsAzureStorageAccountKey" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:WindowsAzureStorageAccountName" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:WindowsAzureStorageAccountName" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:XDriveCacheSizeInMB" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:XDriveCacheSizeInMB" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:XDrivePageBlobName" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:XDrivePageBlobName" />
          </maps>
        </aCS>
        <aCS name="AdminWebSite:XDriveSizeInMB" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSite:XDriveSizeInMB" />
          </maps>
        </aCS>
        <aCS name="AdminWebSiteInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapAdminWebSiteInstances" />
          </maps>
        </aCS>
        <aCS name="Certificate|AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/MapCertificate|AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:AdminWebSite:HttpIn">
          <toPorts>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/HttpIn" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:AdminWebSite:VMManagerServiceExternalHttpPort">
          <toPorts>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/VMManagerServiceExternalHttpPort" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:AdminWebSite:WindowsAzureCompanionHttpIn">
          <toPorts>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/WindowsAzureCompanionHttpIn" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
        <sFSwitchChannel name="SW:AdminWebSite:MySQLBasedDBPort">
          <toPorts>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/MySQLBasedDBPort" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapAdminWebSite:?IsSimulationEnvironment?" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/?IsSimulationEnvironment?" />
          </setting>
        </map>
        <map name="MapAdminWebSite:?RoleHostDebugger?" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/?RoleHostDebugger?" />
          </setting>
        </map>
        <map name="MapAdminWebSite:?StartupTaskDebugger?" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/?StartupTaskDebugger?" />
          </setting>
        </map>
        <map name="MapAdminWebSite:AdminPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/AdminPassword" />
          </setting>
        </map>
        <map name="MapAdminWebSite:AdminUserName" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/AdminUserName" />
          </setting>
        </map>
        <map name="MapAdminWebSite:ApplicationDescription" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/ApplicationDescription" />
          </setting>
        </map>
        <map name="MapAdminWebSite:ApplicationTitle" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/ApplicationTitle" />
          </setting>
        </map>
        <map name="MapAdminWebSite:DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes" />
          </setting>
        </map>
        <map name="MapAdminWebSite:InstallationStatusConfigFileBlob" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/InstallationStatusConfigFileBlob" />
          </setting>
        </map>
        <map name="MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapAdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapAdminWebSite:PHPApplicationsBackupContainerName" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/PHPApplicationsBackupContainerName" />
          </setting>
        </map>
        <map name="MapAdminWebSite:ProductListXmlFeed" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/ProductListXmlFeed" />
          </setting>
        </map>
        <map name="MapAdminWebSite:ProgressInformationFileBlob" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/ProgressInformationFileBlob" />
          </setting>
        </map>
        <map name="MapAdminWebSite:WindowsAzureStorageAccountKey" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/WindowsAzureStorageAccountKey" />
          </setting>
        </map>
        <map name="MapAdminWebSite:WindowsAzureStorageAccountName" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/WindowsAzureStorageAccountName" />
          </setting>
        </map>
        <map name="MapAdminWebSite:XDriveCacheSizeInMB" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/XDriveCacheSizeInMB" />
          </setting>
        </map>
        <map name="MapAdminWebSite:XDrivePageBlobName" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/XDrivePageBlobName" />
          </setting>
        </map>
        <map name="MapAdminWebSite:XDriveSizeInMB" kind="Identity">
          <setting>
            <aCSMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/XDriveSizeInMB" />
          </setting>
        </map>
        <map name="MapAdminWebSiteInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSiteInstances" />
          </setting>
        </map>
        <map name="MapCertificate|AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="AdminWebSite" generation="1" functional="0" release="0" software="C:\Users\cs210student\Desktop\dynovader site\WindowsAzureCompanion\bin\Debug\WindowsAzureCompanion.csx\roles\AdminWebSite" entryPoint="base\x86\WaHostBootstrapper.exe" parameters="base\x86\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="HttpIn" protocol="http" portRanges="80" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="VMManagerServiceExternalHttpPort" protocol="http" portRanges="8081" />
              <inPort name="WindowsAzureCompanionHttpIn" protocol="http" portRanges="8080" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <inPort name="MySQLBasedDBPort" protocol="tcp" portRanges="3306" />
              <outPort name="AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/SW:AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
              <outPort name="AdminWebSite:MySQLBasedDBPort" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/SW:AdminWebSite:MySQLBasedDBPort" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="?IsSimulationEnvironment?" defaultValue="" />
              <aCS name="?RoleHostDebugger?" defaultValue="" />
              <aCS name="?StartupTaskDebugger?" defaultValue="" />
              <aCS name="AdminPassword" defaultValue="" />
              <aCS name="AdminUserName" defaultValue="" />
              <aCS name="ApplicationDescription" defaultValue="" />
              <aCS name="ApplicationTitle" defaultValue="" />
              <aCS name="DiagnosticsAndPerformanceCounterCaptureFrequencyInMinutes" defaultValue="" />
              <aCS name="InstallationStatusConfigFileBlob" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="PHPApplicationsBackupContainerName" defaultValue="" />
              <aCS name="ProductListXmlFeed" defaultValue="" />
              <aCS name="ProgressInformationFileBlob" defaultValue="" />
              <aCS name="WindowsAzureStorageAccountKey" defaultValue="" />
              <aCS name="WindowsAzureStorageAccountName" defaultValue="" />
              <aCS name="XDriveCacheSizeInMB" defaultValue="" />
              <aCS name="XDrivePageBlobName" defaultValue="" />
              <aCS name="XDriveSizeInMB" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;AdminWebSite&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;AdminWebSite&quot;&gt;&lt;e name=&quot;HttpIn&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;e name=&quot;MySQLBasedDBPort&quot; /&gt;&lt;e name=&quot;VMManagerServiceExternalHttpPort&quot; /&gt;&lt;e name=&quot;WindowsAzureCompanionHttpIn&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="ApplicationsAndRuntimeResource" defaultAmount="[2000,2000,2000]" defaultSticky="false" kind="Directory" />
              <resourceReference name="ApplicationsDownloadResource" defaultAmount="[500,500,500]" defaultSticky="false" kind="Directory" />
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="XDriveLocalCache" defaultAmount="[2600,2600,2600]" defaultSticky="false" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSiteInstances" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyID name="AdminWebSiteInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="38a9d08c-593d-4be5-b2e6-389f86d29008" ref="Microsoft.RedDog.Contract\ServiceContract\WindowsAzureCompanionContract@ServiceDefinition.build">
      <interfacereferences>
        <interfaceReference Id="fdfea0cd-6678-48e2-99e1-2bb70d2eced1" ref="Microsoft.RedDog.Contract\Interface\AdminWebSite:HttpIn@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite:HttpIn" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="b22d1bf8-9bf4-4c6a-ac19-0b1b17cf4c95" ref="Microsoft.RedDog.Contract\Interface\AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="81d684c2-49d9-468a-8a36-e83d5f0efd5f" ref="Microsoft.RedDog.Contract\Interface\AdminWebSite:VMManagerServiceExternalHttpPort@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite:VMManagerServiceExternalHttpPort" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="ad8fe647-4919-4d7e-8e98-76488d6b2242" ref="Microsoft.RedDog.Contract\Interface\AdminWebSite:WindowsAzureCompanionHttpIn@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/WindowsAzureCompanion/WindowsAzureCompanionGroup/AdminWebSite:WindowsAzureCompanionHttpIn" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>