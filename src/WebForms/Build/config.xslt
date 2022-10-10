<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>

  <!-- Don't include connectionStrings.configBuilders -->
  <xsl:template match="connectionStrings">
    <xsl:element name="{name()}" >
      <xsl:copy-of select="@*[not(name() = 'configBuilders')]"/>
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <!-- Don't include appSettings.configBuilders -->
  <xsl:template match="appSettings">
    <xsl:element name="{name()}" >
      <xsl:copy-of select="@*[not(name() = 'configBuilders')]"/>
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <!-- Copy appSettings and connectionStrings -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="connectionStrings|add|@*"/>
      <xsl:apply-templates select="appSettings|add|@*"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
