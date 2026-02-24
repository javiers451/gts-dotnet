using Gts.Utils;

namespace Gts.Tests;

public class GtsIdTests
{
    [Fact]
    public void CanBeParsedAsSingleSegmentType()
    {
        var id = GtsId.Parse("gts.vendor.package.namespace.type.v1.0~");

        Assert.True(id.IsType);
        Assert.Single(id.Segments);
    }

    [Fact]
    public void CanBeParsedAsMultipleSegmentsType()
    {
        var id = GtsId.Parse("gts.vendor.package.namespace.type.v1.0~vendor2.package2.namespace2.type2.v1.0~");
        
        Assert.True(id.IsType);
        Assert.Equal(2, id.Segments.Count);
    }

    [Fact]
    public void CanBeParsedAsMultipleSegmentsInstance()
    {
        var id = GtsId.Parse("gts.vendor.package.namespace.type.v1.0~vendor2.package2.namespace2.type2.v1.0");

        Assert.True(id.IsInstance);
        Assert.Equal(2, id.Segments.Count);
    }
    
    [Fact]
    public void CanBeParsedAsSingleSegmentPattern()
    {
        var id = GtsId.ParsePattern("gts.vendor.package.namespace.type.*");

        Assert.False(id.IsType);
        Assert.False(id.IsInstance);
        Assert.True(id.IsPattern);
        Assert.Single(id.Segments);
    }

    [Fact]
    public void CanBeParsedAsMultipleSegmentsPattern()
    {
        var id = GtsId.ParsePattern("gts.vendor.package.namespace.type.v1.0~vendor2.package2.namespace2.type2.*");
        
        Assert.False(id.IsType);
        Assert.False(id.IsInstance);
        Assert.True(id.IsPattern);
        Assert.Equal(2, id.Segments.Count);
    }

    [Fact]
    public void ToGuidProducesGuidInGtsNamespace()
    {
        var id = GtsId.Parse("gts.vendor.package.namespace.type.v1.0~");
        var guid = id.ToGuid();
        
        Assert.Equal(GuidUtils.Create(GuidUtils.GtsNamespace, id.Id), guid);
    }

    [Fact]
    public void ToStringReturnsId()
    {
        var idStr = "gts.vendor.package.namespace.type.v1.0~";
        var id = GtsId.Parse(idStr);
        Assert.Equal(idStr, id.ToString());
        Assert.Equal(id.Id, id.ToString());
    }

    [Fact]
    public void ToStringReturnsPattern()
    {
        var patternStr = "gts.vendor.package.namespace.type.*";
        var pattern = GtsId.ParsePattern(patternStr);
        Assert.Equal(patternStr, pattern.ToString());
        Assert.Equal(pattern.Id, pattern.ToString());
    }
}