---
title: Managing User Settings using xToolkit
caption: User Settings
description: Service to manage user settings with the backward compatibility support in xToolkit
---
This service allows to manage user settings based on the JSON file with the support of backward compatibility.

Define the class which will be storing user settings:

{% code-snippet { file-name: ~utils/UserSettingsDocs.*, regions: [v1] } %}

Call the code below to store the settings into an external file or stream

{% code-snippet { file-name: ~utils/UserSettingsDocs.*, regions: [store] } %}

Settings are serialized using [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)

In order to read the properties call the code below

{% code-snippet { file-name: ~utils/UserSettingsDocs.*, regions: [read] } %}

Quite often it happens that properties structure needs to change (new properties are added, removed or renamed). But clients might already use previous version of the tool with an older settings structure. In this case deserialization of older properties will fail and the tool will not be considered backwards compatible and clients will need to reset the properties which will affect user experience.

This service takes care of this, allowing conversion of the properties.

As an example, let's consider that second version of the user settings declared above has the *Field1* property renamed to *TextField* while the value of Field1 still represents the same entity and it is beneficial to support older properties.

{% code-snippet { file-name: ~utils/UserSettingsDocs.*, regions: [v2] } %}

Even more in the 3rd version of the settings two remaining properties will be renamed as per below.

{% code-snippet { file-name: ~utils/UserSettingsDocs.*, regions: [v3] } %}

So it is now possible to have users with version 1, 2 and 3 of properties. They might upgrade with any of the following paths (if not using the latest version):

* From version 1 to version 2
* From version 2 to version 3
* From version 1 to version 3

In order to handle this scenario it is possible to declare the transformer class and handle each individual version change transforming the [JToken](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JToken.htm)

{% code-snippet { file-name: ~utils/UserSettingsDocs.*, regions: [transformer] } %}

> It is only required to define the transformation between the closest version (i.e. 1 and 2, 2 and 3). It is not required to explicitly define the conversion from 1 to 3. If such conversion is required framework will firstly call the 1 to 2 conversion, followed by 2 to 3.