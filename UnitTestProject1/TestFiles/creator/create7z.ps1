$DD = Resolve-Path "dd\\dd-0.4beta1.exe"
$7ZA = Resolve-Path "7za\\7za.exe"

function SplitFile([String]$from, [String]$rootName, $upperBound) {
    
    $fromFile = [io.file]::OpenRead($from)
    $buff = new-object byte[] $upperBound
    $count = $idx = 0
    try {
        do {
            "Reading $upperBound"
            $count = $fromFile.Read($buff, 0, $buff.Length)
            if ($count -gt 0) {
                $to = "{0}.{1:00000}.{2}" -f ($rootName, $idx, $ext)
                $toFile = [io.file]::OpenWrite($to)
                try {
                    "Writing $count to $to"
                    $tofile.Write($buff, 0, $count)
                } finally {
                    $tofile.Close()
                }
            }
            $idx ++
        } while ($count -gt 0)
    }
    finally {
        $fromFile.Close()
    }
}

pushd ..
foreach ($sample in @(
        @("sample1", 100, 1KB),
        @("sample2", 200, 5KB),
        @("sample3", 300, 50KB)
    )) {
    $folder = $sample[0]
    $count = $sample[1]
    $split = $sample[2]
    
    if (-not (Test-Path $folder)) {
        New-Item -ItemType directory -Path $folder
    }

    for ($i=0; $i -le $count; $i++) {
        $file = $folder + "\" + @("File", "ƒtƒ@ƒCƒ‹")[$i % 2] + "_" + $i + ".dat"
        & $DD if=/dev/random of=$file count=30 bs=1024
    }
    
    $archive = $folder + ".7z"
    if (Test-Path $archive) {
        Remove-Item $archive
    }
    & $7ZA a $archive $folder
    
    $splittedFolder = "splittedfiles_" + $archive
    if (-not (Test-Path $splittedFolder)) {
        New-Item -ItemType directory -Path  $splittedFolder
    } 
    $rootPath = (Resolve-Path $splittedFolder).ToString() + "\" + "split"
    SplitFile (Resolve-Path($archive)) $rootPath $split
}
popd