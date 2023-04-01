# GitStractor - Git Repository Analysis Tool
Project by [Matt Eland](https://LinkedIn.com/in/matteland) ([@IntegerMan](https://twitter.com/IntegerMan))

This project is built for extracting commit, author, and file data from local git repositories in order to visualize repository history and trends to provide insight to software development teams and their stakeholders.

## Project Status

This project is currently usable, but considered in a pre-release state. It is currently missing documentation, polish, and the full range of features I intend to add.

Additionally, the desktop version of GitStractor is not yet available and visualization must be performed in a Jupyter Notebook which will require some setup on your part.

If you are not yet comfortable setting up a Jupyter Notebook, I recommend waiting until the desktop version is available.

## Usage Instructions

GitStractor currently has two components:
- A command line tool for extracting data from a local git repository
- A Jupyter notebook for visualizing the extracted data

### Extracting Commit Data

To get started, you'll need to run the `GitStractor` project in the `GitStractor\GitStractor.sln` solution. This will build the command line tool and place it in the project's binary folder.

Once that file is generated, run the following command to extract data from a local git repository:

    gitstract.exe "C:\Local\Git\Repository\Path"

This will create a series of comma-separated value (CSV) files in the same folder as the `gitstract.exe` file.

### Visualizing Commit Data

Next, open the `GitStractor.ipynb` Jupyter notebook in the `Notebooks` folder. 

In there, you will need to customize the `data_dir` variable to point to the folder where you extracted the data.

I also recommend you change `project_name` to be something appropriate to your project as this value appears on most charts.

## What's Next?

Future efforts on this project will focus on:

- Expanding the range of visualizations available in the Jupyter notebook
- Improving the user experience pulling data from larger repositories
- Investigating Power BI Integration
- Creating a desktop application for extracting and visualizing data

If you'd like to submit a feature request or view the current backlog, please visit the [GitHub Issues tab](https://github.com/IntegerMan/gitstractor/issues)

## Contact

Contact [Matt Eland](https://MattEland.dev) for general questions and feedback.

Please open an issue for enhancement requests and bug reports.
