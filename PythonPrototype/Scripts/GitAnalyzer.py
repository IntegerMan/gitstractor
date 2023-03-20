# We need PyDriller to pull git repository information
from pydriller import Repository
from git import NoSuchPathError

# Pandas is a nice utility and here it allows us to write to CSVs easily
import pandas as pd

def sanitize_message(msg):
    """
    Removes newlines from commit messages and returns the cleansed message
    """
    msg = msg.replace('\r\n', ' ')
    msg = msg.replace('\n', ' ')
    msg = msg.replace(',', '')
    msg = msg.replace('"', '')
    return msg

def get_author(name, email, author_info):
    """
    Gets information about the author from basic information.
    This allows us to standardize formatting and E-Mails for individuals
    """
    email = email.lower()

    for author in author_info:
        if author['email'].lower() == email:
            return author['name'], author['email']
        
        for alias in author['aliases']:
            if alias.lower() == email:
                return author['name'], author['email']
        
    return name, email

def build_commits(repo, author_info):
    """
    Builds lists of commit objects and file commit objects for a repository
    """
    commits = []
    file_commits = []

    for commit in repo.traverse_commits():

        hash = commit.hash
        try:

            # Sanitize the message to prevent it from confusing our resulting CSV
            msg = sanitize_message(commit.msg)

            # Optimization to prevent requesting same data twice
            author_date = commit.author_date
            inserts = commit.insertions
            deletions = commit.deletions           
            project_name = commit.project_name
            project_path = commit.project_path

            # Get Author information
            author = commit.author
            name, email = get_author(author.name, author.email, author_info)

            # Gather individual file commits for granular file analysis
            for f in commit.modified_files:
                if f.new_path is not None:
                    file_commit = {
                        'hash': hash,
                        'message': msg,
                        'author_name': name,
                        'author_email': email,
                        'author_date': author_date,
                        'num_deletes': deletions,
                        'num_inserts': inserts,
                        'net_lines': inserts - deletions,
                        'filename': f.filename,
                        'old_path': f.old_path,
                        'new_path': f.new_path,
                        'project_name': project_name,
                        'project_path': project_path, 

                    }                    
                    file_commits.append(file_commit)

            # Capture information about the commit in object format so I can reference it later
            commit_record = {
                'hash': hash,
                'message': msg,
                'author_name': name,
                'author_email': email,
                'author_date': author_date,
                'num_deletes': deletions,
                'num_inserts': inserts,
                'net_lines': inserts - deletions,
                'num_files': commit.files,
            }
            # Omitted: modified_files (list), project_path, project_name
            commits.append(commit_record)

        except Exception as er:
            print('Problem reading commit ' + hash)
            print(er)
            continue

    return (commits, file_commits)

def correct_renames(file_commits_file_path):
    df = pd.read_csv(file_commits_file_path)

    df.sort_values()

    df.to_csv(file_commits_file_path)


def analyze_repository(path, commits_file_path = 'Commits.csv', 
                       file_commits_file_path = 'FileCommits.csv', 
                       num_threads=1,
                       branch=None,
                       author_info=None):
    """
    Pulls all commits from a git repository using PyDriller.
    NOTE: This can take a LONG time if there are many commits. I'm currently seeing 0.8 seconds per commit on average for remote repositories.
    """

    try:
        # Grab the repository
        print('Analyzing Git Repository at ' + path)
        repo = Repository(path, num_workers=num_threads, only_no_merge=True, order=None, only_in_branch=branch)

        # Read commit data
        print('Fetching commits. This can take a long time...')
        commits, file_commits = build_commits(repo, author_info)
        print('Read ' + str(len(commits)) + ' commits and ' + str(len(file_commits)) + ' file commits')

        # Save the output data
        df_commits = pd.DataFrame(commits)
        df_commits.to_csv(commits_file_path)
        print('Saved to ' + commits_file_path)

        df_file_commits = pd.DataFrame(file_commits)
        df_file_commits.to_csv(file_commits_file_path)
        print('Saved to ' + file_commits_file_path)

        print('Correcting renames')
        correct_renames(file_commits_file_path)
        print('Renames compensated')

        print('Repository Data Pulled Successfully')
    except NoSuchPathError:
        print('Could not find path ' + path)

