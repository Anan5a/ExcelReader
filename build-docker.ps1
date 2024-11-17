# Set source and destination directories
$SOURCE_DIR = "D:\Angular-Learning\Angular-Learning\Assignment\ExcelReader"
$DEST_DIR = ".\ExcelReader\WebUI"

# Check if a tag argument was passed
if ($args.Count -eq 0) {
    Write-Host "You must provide a tag for the Docker image."
    Write-Host "Usage: build.ps1 <tag>"
    exit 1
}

# Get the tag argument
$tag = $args[0]

# Delete the destination directory if it exists and recreate it
if (Test-Path $DEST_DIR) {
    Remove-Item -Recurse -Force $DEST_DIR
}
New-Item -Path $DEST_DIR -ItemType Directory

cp -r $SOURCE_DIR $DEST_DIR

# Remove node_modules from the destination folder
Remove-Item -Recurse -Force "$DEST_DIR\ExcelReader\node_modules"

# Build Docker image with the provided tag
docker build -t "anan5a/excelreader:$tag" -f ExcelReader\Dockerfile .
