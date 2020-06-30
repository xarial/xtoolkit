---
title: Number Box WPF control in xToolkit
caption: Number Box
description: WPF control to support integer and double values
image: number-boxes.png
---
This WPF control is available in 2 version: for double value and integer values.

![Integer and Double number boxes](number-boxes.png)

Bind the View Model property in the XAML to the *Value* dependency property.

{% code-snippet { file-name: ~wpf/NumberBoxDocs.*, regions: [view-model] } %}

{% code-snippet { file-name: ~wpf/NumberBoxControl.xaml, lang: xml } %}
