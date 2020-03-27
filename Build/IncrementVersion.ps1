"ReWrite Version ('$($PSScriptRoot)') ...";

$incrementMajor = $false;
$incrementMinor = $false;
$incrementBuild = $false;
$incrementRevision = $false;
$hasIncrement = $false;
foreach ($arg in $args) {
    if ([System.String]::Equals("Major", $arg, [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $incrementMajor = $true;
        $hasIncrement = $true;
        "Will Increment Major.";
    } elseif ([System.String]::Equals("Minor", $arg, [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $incrementMinor = $true;
        $hasIncrement = $true;
        "Will Increment Minor.";
    } elseif ([System.String]::Equals("Build", $arg, [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $incrementBuild = $true;
        $hasIncrement = $true;
        "Will Increment Build.";
    } elseif ([System.String]::Equals("Revision", $arg, [System.StringComparison]::InvariantCultureIgnoreCase)) {
        $incrementRevision = $true;
        $hasIncrement = $true;
        "Will Increment Revision.";
    }
}
if (!$hasIncrement) {
    "No components were specified to increment. Please specify one or more components to increment:";
    "Major | Minor | Build | Revision";
    return;
}
if (!$incrementRevision) {
    $incrementRevision = $true;
    "Will Increment Revision.";
}

"...";

$fileName = 'Version.props';
$file = Join-Path -ChildPath $fileName -Path $PSScriptRoot;
$xml = New-Object XML;
$xml.Load($file);
$versionMajor = $xml.SelectSingleNode("//VersionMajor").InnerText;
"Current Major: " + $versionMajor;
$versionMinor = $xml.SelectSingleNode("//VersionMinor").InnerText;
"Current Minor: " + $versionMinor;
$versionBuild = $xml.SelectSingleNode("//VersionBuild").InnerText;
"Current Build: " + $versionBuild;
$versionRevision = $xml.SelectSingleNode("//VersionRevision").InnerText;
"Current Revision: " + $versionRevision;

"...";

if ($incrementMajor) {
    $versionMajor = [System.Int32]::Parse($versionMajor) + 1;
    $xml.SelectSingleNode("//VersionMajor").InnerText = $versionMajor;
    "New Major: " + $versionMajor;
}
if ($incrementMinor) {
    $versionMinor = [System.Int32]::Parse($versionMinor) + 1;
    $xml.SelectSingleNode("//VersionMinor").InnerText = $versionMinor;
    "New Minor: " + $versionMinor;
}
if ($incrementBuild) {
    $versionBuild = [System.Int32]::Parse($versionBuild) + 1;
    $xml.SelectSingleNode("//VersionBuild").InnerText = $versionBuild;
    "New Build: " + $versionBuild;
}
if ($incrementRevision) {
    $versionRevision = [System.Int32]::Parse($versionRevision) + 1;
    $xml.SelectSingleNode("//VersionRevision").InnerText = $versionRevision;
    "New Revision: " + $versionRevision;
}

"...";

$version = $versionMajor + '.' + $versionMinor + '.' + $versionBuild + '.' + $versionRevision;
"New Version: " + $version;

"...";

"ReWrite " + $fileName + " ...";
$xml.Save($file);

"...";

$fileName = 'CommonAssemblyInfo.cs';
$file = Join-Path -ChildPath $fileName -Path $PSScriptRoot;
$fileText = [System.IO.File]::ReadAllText($file);
$match = '\s*\(\s*\"[^\"]+\"';
$replacement = '("' + $version + '"';
$attributeName = 'AssemblyVersion';
$fileText = [System.Text.RegularExpressions.Regex]::Replace(
        $fileText,
        $attributeName + $match,
        $attributeName + $replacement);
$attributeName = 'AssemblyFileVersion';
$fileText = [System.Text.RegularExpressions.Regex]::Replace(
        $fileText,
        $attributeName + $match,
        $attributeName + $replacement);
$attributeName = 'AssemblyInformationalVersion';
$fileText = [System.Text.RegularExpressions.Regex]::Replace(
        $fileText,
        $attributeName + $match,
        $attributeName + $replacement);
"ReWrite " + $fileName + " ...";
[System.IO.File]::WriteAllText($file, $fileText);

"Done.";
