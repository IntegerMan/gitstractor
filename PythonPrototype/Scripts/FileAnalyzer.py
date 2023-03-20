# Import required libraries
import pandas as pd
import os

# Common source file extensions. This list is very incomplete. Intentionally not including JSON / XML
source_extensions = [
    '.cs', 
    '.vb', 
    '.py',
    '.java', 
    '.r', 
    '.agc', 
    '.fs', 
    '.js', 
    '.cpp', 
    '.go', 
    '.aspx', 
    '.jsp', 
    '.do', 
    '.php', 
    '.ipynb', 
    '.sh', 
    '.html', 
    '.lua', 
    '.css',
    '.dib',
    '.md',
    '.sln'
    ]

# Some directories should be ignored as their output is not helpful
ignored_directories = [
    '.git', 
    '.vs', 
    '.vscode',
    '.idea',
    'obj', 
    'ndependout', 
    'bin', 
    'debug', 
    'release',
    'node_modules', 
    '__pycache__'
    ]


def is_source_file(file_label):
    """
    Defines what a source file is.
    """
    file, _, _ = file_label
    _, ext = os.path.splitext(file)

    return ext.lower() in source_extensions

def count_lines(path):
    """
    Reads the file at the specified path and returns the number of lines in that file
    Code taken from https://stackoverflow.com/questions/845058/how-to-get-line-count-of-a-large-file-cheaply-in-python
    """

    def _make_gen(reader):
        b = reader(2 ** 16)
        while b:
            yield b
            b = reader(2 ** 16)

    with open(path, "rb") as f:
        count = sum(buf.count(b"\n") for buf in _make_gen(f.raw.read))

    return count

def get_file_metrics(files, root):
    """
    This function gets all metrics for the files and returns them as a list of file detail objects
    """
    results = []

    for file, folders, relative_path in files:
        lines = count_lines(file) # Slow as it actually reads the file
        _, filename = os.path.split(file)
        _, ext = os.path.splitext(filename)

        fullpath = ''
        area = 'root'

        if folders != None and len(folders) > 0:
            project = folders[0]

            for folder in folders[1:]:
                print(folder)
                if len(fullpath) > 0:
                    fullpath += '/'
                fullpath += folder
        else:
            project = ''

        if len(fullpath) <= 0:
            fullpath = '.'

        id = root + project + '/' + fullpath + '/' + filename

        if relative_path == '':
            rel_path = filename
        else:
            rel_path = relative_path + '\\' + filename

        file_details = {
                        'fullpath': id,
                        'root': root,
                        'project': project,
                        'path': fullpath,
                        'area': area,
                        'relative_path': rel_path,
                        'filename': filename,
                        'ext': ext,
                        'lines': lines,
                        }
        results.append(file_details)

    return results

def get_file_list(dir_path, relative_path, paths=None):
    """
    Gets a list of files in this this directory and all contained directories
    """

    files = list()
    contents = os.listdir(dir_path)

    for entry in contents:
        path = os.path.join(dir_path, entry)
        if os.path.isdir(path):
            # Ignore build and reporting directories
            if entry.lower() in ignored_directories:
                continue

            if relative_path == '':
                new_path = entry
            else:
                new_path = relative_path + '\\' + entry

            # Maintain an accurate, but separate, hierarchy array
            if paths is None:
                p = [entry]
            else:
                p = paths[:]

            files = files + get_file_list(path, new_path, p)
        else:
            files.append((path, paths, relative_path))
    return files

def get_source_file_metrics(root):
    """
    This function gets all source files and metrics associated with them from a given path
    """
    source_files = filter(is_source_file, get_file_list(root, ''))
    return get_file_metrics(list(source_files), root)

def extract_area(row):
    rel_path = row['relative_path']

    row['area'] = 'Root'
    if not rel_path == None and rel_path.__contains__('\\'):
        parts = rel_path.split('\\')
        if len(parts) > 2:
            row['area'] = parts[1]
    return row

def build_file_sizes(directory, output_file_path='FileSizes.csv'):
    """
    Analyzes all source files in the given directory and its subdirectory and returns a list of analysis results
    """
    try:
        print('Reading file metrics from ' + directory)

        files = get_source_file_metrics(directory)
        print(str(len(files)) + " source files read from " + directory)

        # Introduce an area column
        df = pd.DataFrame(files)
        df = df.apply(extract_area, axis=1)

        # Write to a file for other analyses processes
        df.to_csv(output_file_path)
        print('File size information saved to ' + output_file_path)
    except FileNotFoundError:
        print('Could not find directory ' + directory)