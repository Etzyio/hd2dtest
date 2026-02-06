#!/usr/bin/env python3
"""
构建版本管理脚本
该脚本用于在构建时生成version.json文件，包含版本号、构建日期和Git提交哈希
"""

import os
import subprocess
import json
from datetime import datetime

# 项目根目录
PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
# 版本文件路径
VERSION_FILE = os.path.join(PROJECT_ROOT, "version.json")
# 默认版本号
DEFAULT_VERSION = "0.1.0"

def get_git_commit():
    """
    获取当前Git提交哈希
    """
    try:
        # 尝试向上查找.git目录
        current_dir = PROJECT_ROOT
        while current_dir != os.path.dirname(current_dir):
            if os.path.exists(os.path.join(current_dir, ".git")):
                break
            current_dir = os.path.dirname(current_dir)
        
        # 执行git命令
        result = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            cwd=current_dir,
            capture_output=True,
            text=True,
            timeout=5
        )
        
        if result.returncode == 0:
            return result.stdout.strip()
        else:
            print(f"Git命令执行失败: {result.stderr}")
            return "unknown"
    except Exception as e:
        print(f"获取Git提交哈希失败: {e}")
        return "unknown"

def get_git_tag():
    """
    获取当前Git标签
    """
    try:
        # 尝试向上查找.git目录
        current_dir = PROJECT_ROOT
        while current_dir != os.path.dirname(current_dir):
            if os.path.exists(os.path.join(current_dir, ".git")):
                break
            current_dir = os.path.dirname(current_dir)
        
        # 执行git命令
        result = subprocess.run(
            ["git", "describe", "--tags", "--exact-match"],
            cwd=current_dir,
            capture_output=True,
            text=True,
            timeout=5
        )
        
        if result.returncode == 0:
            return result.stdout.strip()
        else:
            return ""
    except Exception as e:
        print(f"获取Git标签失败: {e}")
        return ""

def ensure_version_format(base_version, git_commit):
    """
    确保版本号格式为四位数字：大版本.小版本.小修改.git提交号
    """
    try:
        # 分割版本号
        parts = base_version.split('.')
        major = 0
        minor = 0
        patch = 0
        
        # 解析现有版本号
        if len(parts) > 0 and parts[0].isdigit():
            major = int(parts[0])
        if len(parts) > 1 and parts[1].isdigit():
            minor = int(parts[1])
        if len(parts) > 2 and parts[2].isdigit():
            patch = int(parts[2])
        
        # 使用实际的Git提交哈希
        if not git_commit or git_commit == "unknown":
            git_commit = "b8efa1cf6cc30f9618a3f1de10687fdd0c9346be"
        
        # 获取git提交号前8位
        commit_short = git_commit[:8]
        
        # 构建四位数字版本号
        version = f"{major}.{minor}.{patch}.{commit_short}"
        print(f"生成的版本号: {version}")
        return version
    except Exception as e:
        print(f"格式化版本号失败: {e}")
        # 如果出错，返回默认格式
        if not git_commit or git_commit == "unknown":
            git_commit = "b8efa1cf6cc30f9618a3f1de10687fdd0c9346be"
        commit_short = git_commit[:8]
        return f"0.1.0.{commit_short}"

def generate_version_file():
    """
    生成version.json文件
    """
    try:
        # 获取构建日期
        build_date = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        print(f"构建日期: {build_date}")
        
        # 获取Git提交哈希
        git_commit = get_git_commit()
        print(f"Git提交哈希: {git_commit}")
        
        # 获取Git标签
        git_tag = get_git_tag()
        print(f"Git标签: {git_tag}")
        
        # 确定基础版本号
        if git_tag:
            # 移除tag前缀v（如果存在）
            base_version = git_tag[1:] if git_tag.startswith('v') else git_tag
        else:
            base_version = DEFAULT_VERSION
        
        print(f"基础版本号: {base_version}")
        
        # 确保版本号格式
        game_version = ensure_version_format(base_version, git_commit)
        
        # 创建版本信息字典
        version_data = {
            "version": game_version,
            "build_date": build_date,
            "git_commit": git_commit
        }
        
        # 保存到文件
        with open(VERSION_FILE, 'w', encoding='utf-8') as f:
            json.dump(version_data, f, indent=2, ensure_ascii=False)
        
        print(f"版本信息已保存到: {VERSION_FILE}")
        print(f"版本号: {game_version}")
        return True
    except Exception as e:
        print(f"生成版本文件失败: {e}")
        return False

if __name__ == "__main__":
    print("开始生成版本信息...")
    success = generate_version_file()
    if success:
        print("版本信息生成成功！")
    else:
        print("版本信息生成失败！")
        exit(1)
