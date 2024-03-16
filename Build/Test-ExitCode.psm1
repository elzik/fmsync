function Test-ExitCode 
{
    if ($LastExitCode -ne 0) 
    {
        throw "The previous executable command failed. See console output prior to this message for details."
    }        
}

Export-ModuleMember -Function Test-ExitCode