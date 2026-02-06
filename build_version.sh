#!/bin/bash
"""
构建版本管理脚本
该脚本用于在构建时生成version.json文件，包含版本号、构建日期和Git提交哈希
"""

# 项目根目录
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# 版本文件路径
VERSION_FILE="${PROJECT_ROOT}/version.json"
# 默认版本号
DEFAULT_VERSION="0.1.0"
# 默认Git提交哈希
DEFAULT_GIT_COMMIT="b8efa1cf6cc30f9618a3f1de10687fdd0c9346be"

# 获取当前Git提交哈希
get_git_commit() {
    local current_dir="${PROJECT_ROOT}"
    
    # 尝试向上查找.git目录
    while [[ "${current_dir}" != "$(dirname "${current_dir}")" ]]; do
        if [[ -d "${current_dir}/.git" ]]; then
            break
        fi
        current_dir="$(dirname "${current_dir}")"
    done
    
    # 执行git命令
    if git -C "${current_dir}" rev-parse HEAD &>/dev/null; then
        git -C "${current_dir}" rev-parse HEAD
    else
        echo "Git命令执行失败" >&2
        echo "unknown"
    fi
}

# 获取当前Git标签
get_git_tag() {
    local current_dir="${PROJECT_ROOT}"
    
    # 尝试向上查找.git目录
    while [[ "${current_dir}" != "$(dirname "${current_dir}")" ]]; do
        if [[ -d "${current_dir}/.git" ]]; then
            break
        fi
        current_dir="$(dirname "${current_dir}")"
    done
    
    # 执行git命令
    if git -C "${current_dir}" describe --tags --exact-match &>/dev/null; then
        git -C "${current_dir}" describe --tags --exact-match
    else
        echo ""
    fi
}

# 确保版本号格式为四位数字：大版本.小版本.小修改.git提交号
ensure_version_format() {
    local base_version="$1"
    local git_commit="$2"
    
    # 分割版本号
    IFS='.' read -ra parts <<< "${base_version}"
    local major=0
    local minor=0
    local patch=0
    
    # 解析现有版本号
    if [[ ${#parts[@]} -gt 0 ]] && [[ "${parts[0]}" =~ ^[0-9]+$ ]]; then
        major="${parts[0]}"
    fi
    if [[ ${#parts[@]} -gt 1 ]] && [[ "${parts[1]}" =~ ^[0-9]+$ ]]; then
        minor="${parts[1]}"
    fi
    if [[ ${#parts[@]} -gt 2 ]] && [[ "${parts[2]}" =~ ^[0-9]+$ ]]; then
        patch="${parts[2]}"
    fi
    
    # 使用实际的Git提交哈希
    if [[ -z "${git_commit}" ]] || [[ "${git_commit}" == "unknown" ]]; then
        git_commit="${DEFAULT_GIT_COMMIT}"
    fi
    
    # 获取git提交号前8位
    local commit_short="${git_commit:0:8}"
    
    # 构建四位数字版本号
    local version="${major}.${minor}.${patch}.${commit_short}"
    echo "生成的版本号: ${version}" >&2
    echo "${version}"
}

# 生成version.json文件
generate_version_file() {
    # 获取构建日期
    local build_date
    build_date=$(date '+%Y-%m-%d %H:%M:%S')
    echo "构建日期: ${build_date}"
    
    # 获取Git提交哈希
    local git_commit
    git_commit=$(get_git_commit)
    echo "Git提交哈希: ${git_commit}"
    
    # 获取Git标签
    local git_tag
    git_tag=$(get_git_tag)
    echo "Git标签: ${git_tag}"
    
    # 确定基础版本号
    local base_version
    if [[ -n "${git_tag}" ]]; then
        # 移除tag前缀v（如果存在）
        if [[ "${git_tag}" == v* ]]; then
            base_version="${git_tag:1}"
        else
            base_version="${git_tag}"
        fi
    else
        base_version="${DEFAULT_VERSION}"
    fi
    
    echo "基础版本号: ${base_version}"
    
    # 确保版本号格式
    local game_version
    game_version=$(ensure_version_format "${base_version}" "${git_commit}")
    
    # 创建JSON内容
    local json_content
    json_content=$(cat <<EOF
{
  "version": "${game_version}",
  "build_date": "${build_date}",
  "git_commit": "${git_commit}"
}
EOF
)
    
    # 保存到文件
    echo "${json_content}" > "${VERSION_FILE}"
    
    echo "版本信息已保存到: ${VERSION_FILE}"
    echo "版本号: ${game_version}"
    return 0
}

# 主函数
main() {
    echo "开始生成版本信息..."
    
    if generate_version_file; then
        echo "版本信息生成成功！"
        exit 0
    else
        echo "版本信息生成失败！"
        exit 1
    fi
}

# 执行主函数
main "$@"
