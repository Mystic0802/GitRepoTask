## Overview
This project is a **ASP.NET MVC 5** application built on **.NET Framework 4.8**. It serves as a technical test project that allows users to search for GitHub profiles and display relevant details, including their **top 5 most starred repositories**.

## Getting Started
### Prerequisites
- **.NET Framework 4.8**
- **Visual Studio** 
- **Git**

### Installation & Running the Project
1. **Clone the repository**:
   ```sh
   git clone https://github.com/Mystic0802/GitRepoTask
   ```
2. **Open the project in Visual Studio**.
3. **Run the application** (It should work without additional configurations).

### Troubleshooting
If you encounter the following error:
```
Could not find a part of the path ... bin\roslyn\csc.exe
```
Try running the following command in the **Package Manager Console**:
```powershell
Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -r
```
Alternatively, refer to possible solutions here: [StackOverflow Thread](https://stackoverflow.com/questions/32780315/could-not-find-a-part-of-the-path-bin-roslyn-csc-exe).
