using System.Text.Json.Nodes;
using Gts.Extraction;

namespace Gts.Tests.Extraction;

public class ExtractEntityTests
{
    [Fact]
    public void ExtractingPopulatesRefsWithIds()
    {
        var entity = GtsExtract.ExtractEntity(new JsonObject
        {
            ["$id"] = "gts.x.test.core.schema.v1~",
            ["$ref"] = "gts.x.test.core.base.v1~",
        });
        
        Assert.Contains(entity.GtsRefs,
            r => r is { Id: "gts.x.test.core.schema.v1~", SourcePath: "$id" });
    }
    
    [Fact]
    public void ExtractingPopulatesRefsWithExplicitRefs()
    {
        var entity = GtsExtract.ExtractEntity(new JsonObject
        {
            ["$id"] = "gts.x.test.core.schema.v1~",
            ["$ref"] = "gts.x.test.core.base.v1~",
        });
        
        Assert.Contains(entity.GtsRefs,
            r => r is { Id: "gts.x.test.core.base.v1~", SourcePath: "$ref" });
    }
    
    [Fact]
    public void ExtractingPopulatesRefsWithExplicitNestedObjectRefs()
    {
        var entity = GtsExtract.ExtractEntity(new JsonObject
        {
            ["$id"] = "gts.x.test.core.schema.v1~",
            ["properties"] = new JsonObject
            {
                ["field1"] = new JsonObject
                {
                    ["$ref"] = "gts.x.test.core.field.v1~",
                },
            },
        });
        
        Assert.Contains(entity.GtsRefs,
            r => r is { Id: "gts.x.test.core.field.v1~", SourcePath: "properties.field1.$ref" });
    }
    
    [Fact]
    public void ExtractingPopulatesRefsWithExplicitNestedArrayRefs()
    {
        var entity = GtsExtract.ExtractEntity(new JsonObject
        {
            ["$id"] = "gts.x.test.core.schema.v1~",
            ["items"] = new JsonArray
            {
                new JsonObject
                {
                    ["$ref"] = "gts.x.test.core.item.v1~",
                }
            },
        });
        
        Assert.Contains(entity.GtsRefs,
            r => r is { Id: "gts.x.test.core.item.v1~", SourcePath: "items[0].$ref" });
    }
}