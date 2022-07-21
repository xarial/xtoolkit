# How To Test AssemblyResolver

* Compile the **ConsoleTester** project. This project is referencing **Lib** dll with version 1.0.0.0
* Copy and overwrite the files from the **!** folder into the build folder
    * **Lib.dll** reference is of version 1.1.0.0
    * **_ConsoleTester.exe.config** provides binding redirect to this version. Note it is required to keep this file a different name from the default (e.g. **ConsoleTester.exe.config**) otherwise built-in binding redirect will be used from the .exe file and the **AssemblyResolver** will not be called (not the case for the .dll projects)
~~~ xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1" />
  </startup>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Lib" publicKeyToken="edd202a4e66b8d76" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.1.0.0" newVersion="1.1.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
~~~
* Run the **ConsoleTester.exe** from the Windows File Explorer and attach to the process to debug
    * This is done so the default build process is not overriding the target files