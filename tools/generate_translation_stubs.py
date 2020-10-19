# -*- coding: utf-8 -*-
import os
import re
from shutil import copyfile

# --------------- Configuration ---------------- #
master_file = "./ModAssistant/Localisation/en.xaml"
source_dir = "./ModAssistant/Localisation"
target_dir = source_dir
comment = " <!-- NEEDS TRANSLATING -->"
# --------------- Configuration ---------------- #


def read_file(filepath):
    """
    Read file content
    :param filepath:
    :return:
    """
    with open(filepath, "r", encoding="utf-8") as file:
        data = file.read()
    return data


def find_files(root_directory):
    """
    Find all files to be processed
    :param root_directory:
    :return:
    """
    for root, ds, fs in os.walk(root_directory):
        for fn in fs:
            fullname = os.path.join(root, fn)
            master_name = os.path.basename(master_file)
            if master_name in fullname:
                continue
            yield fullname


def search(regex, m_string, group=1):
    """
    Search for the first item that matches by regular expression
    :param regex:
    :param m_string:
    :return:
    """
    found = re.compile(regex, re.M).search(m_string)
    result = ""
    if found:
        result = found.group(group)
    return result


def write_file(filepath, content):
    """
    Write text content to file
    :param filepath:
    :param content:
    :return:
    """
    if not os.path.exists(target_dir):
        os.makedirs(target_dir, exist_ok=True)
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(content)


def find_items(data):
    keys_regex = r"<\s*([^\s>]+)\s.*Key[^>]+>"
    keys_matches = re.finditer(keys_regex, data, re.M)
    items = []
    for n, m in enumerate(keys_matches, start=1):
        if len(items):
            last_dict = items[-1]
            if len(last_dict):
                last_dict['end'] = m.start()
        items.append({"match": m[0], "name": m[1], "start": m.end()})
    items[-1]['end'] = len(data)
    for item in items:
        item_text = data[item['start']: item['end']]
        end_tag = "</%s>" % item['name']
        full_tag = item['match'] + item_text[:item_text.rfind(end_tag) + len(end_tag)]
        item['end_tag'] = end_tag
        item['full_tag'] = full_tag
    return items


def wrapper_key(match, content):
    """
    Try to find the comment behind the key
    :param match:
    :param content:
    :return:
    """
    regex = "(\s*" + re.escape(match) + "(\ *<\!\-\-[^\-]+\-\->)?\s*)"
    with_comment = search(r"" + str(regex), content, 1)
    # print(regex)
    # print(match)
    # print(with_comment)
    # print("-----")
    if with_comment:
        match = with_comment
    return match


def main():
    """
    Cycle processing of xaml files except master_file
    :return:
    """
    master_data = read_file(master_file)
    print("The master file is", master_file)
    items = find_items(master_data)

    for f in find_files(source_dir):
        content = master_data + ""
        xml_data = read_file(f)
        xml_dict = {}
        for xml_item in find_items(xml_data):
            xml_dict[xml_item['match']] = xml_item['full_tag']
        for item in items:
            # print(f)
            # if "OneClick:Done" in item['match'] and "fr.xaml" in f:
            #     print("----")
            #     pass
            if item['match'] in xml_dict.keys():
                match = wrapper_key(xml_dict[item['match']], xml_data)
                master_match = wrapper_key(item['full_tag'], content)
                # print(master_match)
                content = content.replace(master_match, match)
            else:
                pre_blanks = search(r"(\s*)" + re.escape(item['full_tag']), master_data)
                content = re.sub(r"\s*" + re.escape(item['full_tag']), pre_blanks + item['full_tag'] + comment, content)
        # Put the processed files in the "dist" directory
        filepath = f.replace(source_dir, target_dir)
        write_file(filepath, content)
        print("Processing", f)
    # Copy "master_file" to the target directory
    if source_dir != target_dir:
        copyfile(master_file, master_file.replace(source_dir, target_dir))


if __name__ == '__main__':
    main()
    print("done'd!")
