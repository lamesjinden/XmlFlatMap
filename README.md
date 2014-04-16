#XmlFlatMap
==========

flat = FlatMap(xml, ... )

XML document processor, provided as a MSBuild Task. Provides 'flatmap' application over XML input. Intended for exploding MSBuild project files according to composite values of ItemGroup element metadata. Because MSBuild just isn't lisp-y enough.

From:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <ItemGroup>
        <Download Include="http://www.cnn.com">
            <Server>1,2,3</Server>
        </Download>
    </ItemGroup>
</Project>
```

To:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <ItemGroup>
        <Download Include="http://www.cnn.com">
            <Server>1</Server>
        </Download>
        <Download Include="http://www.cnn.com">
            <Server>2</Server>
        </Download>
        <Download Include="http://www.cnn.com">
            <Server>3</Server>
        </Download>
    </ItemGroup>
</Project>
```

