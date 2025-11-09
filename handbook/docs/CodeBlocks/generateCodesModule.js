const fs = require('fs');
const path = require('path');

// 配置要搜索的目录列表（相对于项目根目录）
const SEARCH_DIRECTORIES = [
  'examples',     // 示例代码目录
];

// 排除的目录模式
const EXCLUDE_PATTERNS = ['obj', 'bin', '.vs', 'packages', 'node_modules'];

/**
 * 递归搜索目录下的所有.cs文件
 * @param {string} dirPath - 目录路径
 * @param {string[]} excludePatterns - 排除的路径模式
 * @returns {string[]} - .cs文件路径数组
 */
function findCsFiles(dirPath, excludePatterns = EXCLUDE_PATTERNS)
{
  const files = [];

  if (!fs.existsSync(dirPath))
  {
    console.warn(`⚠️ 目录不存在: ${dirPath}`);
    return files;
  }

  const items = fs.readdirSync(dirPath);

  for (const item of items)
  {
    const itemPath = path.join(dirPath, item);
    const stat = fs.statSync(itemPath);

    // 检查是否需要排除
    const shouldExclude = excludePatterns.some(pattern =>
      itemPath.toLowerCase().includes(pattern.toLowerCase())
    );

    if (shouldExclude)
    {
      continue;
    }

    if (stat.isDirectory())
    {
      // 递归搜索子目录
      files.push(...findCsFiles(itemPath, excludePatterns));
    } else if (item.toLowerCase().endsWith('.cs'))
    {
      files.push(itemPath);
    }
  }

  return files;
}

/**
 * 读取多个目录的cs文件并合并内容
 * @returns {Object} - 合并后的代码内容和文件信息
 */
function readAndMergeCsFiles()
{
  const projectRoot = path.join(__dirname, '..', '..', '..');

  console.log('🚀 开始搜索配置的目录...');
  console.log('📂 配置的搜索目录：');
  SEARCH_DIRECTORIES.forEach(dir => console.log(`   - ${dir}`));

  let allCsFiles = [];
  let validDirectories = [];

  // 遍历所有配置的目录
  for (const searchDir of SEARCH_DIRECTORIES)
  {
    const targetDir = path.join(projectRoot, searchDir);

    if (!fs.existsSync(targetDir))
    {
      console.warn(`⚠️ 目录不存在，跳过: ${searchDir}`);
      continue;
    }

    console.log(`🔍 搜索目录: ${targetDir}`);
    const csFiles = findCsFiles(targetDir);

    if (csFiles.length > 0)
    {
      console.log(`   找到 ${csFiles.length} 个 .cs 文件`);
      allCsFiles.push(...csFiles);
      validDirectories.push(searchDir);
    } else
    {
      console.log(`   该目录下没有 .cs 文件`);
    }
  }

  if (allCsFiles.length === 0)
  {
    console.warn(`⚠️ 在配置的目录中没有找到任何 .cs 文件`);
    return { content: '', files: [], directories: [] };
  }

  console.log(`\n📁 总计找到 ${allCsFiles.length} 个 .cs 文件:`);

  let mergedContent = '';
  const fileInfos = [];

  for (const filePath of allCsFiles)
  {
    try
    {
      const content = fs.readFileSync(filePath, 'utf8');
      const relativePath = path.relative(projectRoot, filePath);

      console.log(`   - ${relativePath} (${content.length} 字符)`);

      // 添加文件标识注释
      const fileHeader = `\n// ===== FILE: ${relativePath} =====\n`;
      mergedContent += fileHeader + content + '\n';

      fileInfos.push({
        path: filePath,
        relativePath: relativePath,
        size: content.length
      });
    } catch (error)
    {
      console.error(`❌ 读取文件失败 ${filePath}: ${error.message}`);
    }
  }

  return {
    content: mergedContent,
    files: fileInfos,
    directories: validDirectories
  };
}

/**
 * 生成JavaScript模块，搜索所有配置的目录
 */
function generateCodesModule()
{
  try
  {
    console.log('🚀 开始生成代码模块...');

    const { content: codesContent, files: fileInfos, directories: validDirectories } = readAndMergeCsFiles();

    if (!codesContent.trim())
    {
      console.error('❌ 没有找到任何代码内容');
      process.exit(1);
    }

    const moduleContent = `// 自动生成的文件 - 请勿手动编辑
// 由 generateCodesModule.js 生成
// 搜索目录: ${SEARCH_DIRECTORIES.join(', ')}
// 有效目录: ${validDirectories.join(', ')}
// 包含文件: ${fileInfos.map(f => f.relativePath).join(', ')}

export const codesContent = ${JSON.stringify(codesContent)};

// 文件信息
export const fileInfos = ${JSON.stringify(fileInfos, null, 2)};

// 搜索目录配置
export const searchDirectories = ${JSON.stringify(SEARCH_DIRECTORIES)};
export const validDirectories = ${JSON.stringify(validDirectories)};

/**
 * 解析高亮语法
 * @param {string} highlightStr - 高亮字符串，如 "{1,2-3,5}"
 * @returns {number[]} - 高亮行数组
 */
function parseHighlightSyntax(highlightStr) {
  if (!highlightStr) return [];
  
  // 移除大括号
  const content = highlightStr.replace(/[{}]/g, '');
  if (!content) return [];
  
  const lines = [];
  const parts = content.split(',');
  
  for (const part of parts) {
    const trimmed = part.trim();
    if (trimmed.includes('-')) {
      // 范围语法，如 "2-3"
      const [start, end] = trimmed.split('-').map(num => parseInt(num.trim()));
      if (!isNaN(start) && !isNaN(end) && start <= end) {
        for (let i = start; i <= end; i++) {
          lines.push(i);
        }
      }
    } else {
      // 单行语法，如 "1"
      const num = parseInt(trimmed);
      if (!isNaN(num)) {
        lines.push(num);
      }
    }
  }
  
  // 去重并排序
  return [...new Set(lines)].sort((a, b) => a - b);
}

/**
 * 从代码内容中提取指定region的代码
 * @param {string} regionTitle - region的名称
 * @returns {Object|null} - 提取的代码块信息或null
 */
export function extractCodeRegion(regionTitle) {
  const content = codesContent;
  const lines = content.split('\\n');
  
  // 转义特殊字符以用于正则表达式  
  const escapeRegex = (str) => {
    return str.replace(/[.*+?^\\$\\{\\}()|]/g, '\\\\\\\\$&')
              .replace(/\\[/g, '\\\\\\\\[')
              .replace(/\\]/g, '\\\\\\\\]')
              .replace(/\\\\\\\\/g, '\\\\\\\\\\\\\\\\');
  };
  const escapedTitle = escapeRegex(regionTitle);
  
  // 修改正则表达式以支持高亮语法和尾部空白(\\r等)
  // 匹配 #region RegionName {1,2-3} 或 #region RegionName
  const regionStartPattern = new RegExp(\`^\\\\s*#region\\\\s+\${escapedTitle}(?:\\\\s*\\\\{([^}]+)\\\\})?\\\\s*$\`);
  const anyRegionStartPattern = /^\\s*#region\\s+(.+?)(?:\\s*\\{[^}]+\\})?\\s*$/;
  const regionEndPattern = /^\\s*#endregion(?:\\s+.*?)?\\s*$/;
  
  let startIndex = -1;
  let endIndex = -1;
  let sourceFile = null;
  let highlightLines = [];
  
  // 找到目标region开始位置
  for (let i = 0; i < lines.length; i++) {
    const match = regionStartPattern.exec(lines[i]);
    if (match) {
      startIndex = i + 1; // 跳过#region行
      
      // 解析高亮语法
      if (match[1]) {
        highlightLines = parseHighlightSyntax('{' + match[1] + '}');
      }
      
      // 查找region所在的源文件
      for (let j = i; j >= 0; j--) {
        const line = lines[j];
        const fileMatch = line.match(/^\\/\\/ ===== FILE: (.+) =====$/);
        if (fileMatch) {
          sourceFile = fileMatch[1];
          break;
        }
      }
      break;
    }
  }
  
  if (startIndex === -1) {
    return null; // 没找到对应的region
  }
  
  // 找到对应的#endregion，需要处理嵌套情况
  let regionDepth = 1; // 当前region深度，从1开始（因为已经进入了目标region）
  
  for (let i = startIndex; i < lines.length; i++) {
    const line = lines[i];
    
    // 检查是否遇到了新的#region（嵌套）
    if (anyRegionStartPattern.test(line)) {
      regionDepth++; // 进入更深层的region
    } 
    // 检查是否遇到了#endregion
    else if (regionEndPattern.test(line)) {
      regionDepth--; // 退出一层region
      
      // 如果深度回到0，说明找到了目标region的结束位置
      if (regionDepth === 0) {
        endIndex = i;
        break;
      }
    }
  }
  
  if (endIndex === -1) {
    return null; // 没找到对应的#endregion
  }
  
  // 提取代码块并移除多余的空白行
  const codeLines = lines.slice(startIndex, endIndex);
  
  // 移除开头和结尾的空行
  while (codeLines.length > 0 && codeLines[0].trim() === '') {
    codeLines.shift();
  }
  while (codeLines.length > 0 && codeLines[codeLines.length - 1].trim() === '') {
    codeLines.pop();
  }
  
  // 统一缩进处理
  let code = '';
  if (codeLines.length > 0) {
    // 找到最小缩进
    const minIndent = codeLines
      .filter(line => line.trim() !== '')
      .reduce((min, line) => {
        const indent = line.match(/^\\s*/)[0].length;
        return Math.min(min, indent);
      }, Infinity);
    
    // 移除统一的缩进
    if (minIndent > 0 && minIndent !== Infinity) {
      code = codeLines.map(line => line.slice(minIndent)).join('\\n');
    } else {
      code = codeLines.join('\\n');
    }
  }
  
  return {
    code: code,
    sourceFile: sourceFile || 'unknown',
    startLine: startIndex,
    endLine: endIndex,
    highlightLines: highlightLines  // 添加高亮行信息
  };
}

/**
 * 获取所有可用的region列表
 * @returns {Array} - region信息数组，包含名称和来源文件
 */
export function getAvailableRegions() {
  const content = codesContent;
  const lines = content.split('\\n');
  const regionPattern = /^\\s*#region\\s+(.+)\\s*$/;
  const regions = [];
  let currentFile = null;
  
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    
    // 检查文件标识
    const fileMatch = line.match(/^\\/\\/ ===== FILE: (.+) =====$/);
    if (fileMatch) {
      currentFile = fileMatch[1];
      continue;
    }
    
    // 检查region
    const regionMatch = line.match(regionPattern);
    if (regionMatch) {
      regions.push({
        name: regionMatch[1].trim(),
        file: currentFile || 'unknown',
        line: i + 1
      });
    }
  }
  
  return regions;
}
`;

    const outputPath = path.join(__dirname, 'codesData.js');
    fs.writeFileSync(outputPath, moduleContent, 'utf8');

    console.log('✅ 代码内容已成功生成到 codesData.js');
    console.log(`📁 输出文件: ${outputPath}`);
    console.log(`� 搜索的目录: ${SEARCH_DIRECTORIES.join(', ')}`);
    console.log(`✅ 有效目录: ${validDirectories.join(', ')}`);
    console.log(`�📊 合并了 ${fileInfos.length} 个文件，总计 ${codesContent.length} 个字符`);

    // 显示找到的regions
    const availableRegions = getAvailableRegions(codesContent);
    if (availableRegions.length > 0)
    {
      console.log('🔍 找到以下代码区域:');
      availableRegions.forEach(region =>
      {
        console.log(`   - ${region.name} (来自: ${region.file})`);
      });
    }

  } catch (error)
  {
    console.error('❌ 生成失败:', error.message);
    process.exit(1);
  }
}

/**
 * 辅助函数：从代码内容中获取regions
 */
function getAvailableRegions(content)
{
  const lines = content.split('\n');
  const regionPattern = /^\s*#region\s+(.+)\s*$/;
  const regions = [];
  let currentFile = null;

  for (let i = 0; i < lines.length; i++)
  {
    const line = lines[i];

    // 检查文件标识
    const fileMatch = line.match(/^\/\/ ===== FILE: (.+) =====$/);
    if (fileMatch)
    {
      currentFile = fileMatch[1];
      continue;
    }

    // 检查region
    const regionMatch = line.match(regionPattern);
    if (regionMatch)
    {
      regions.push({
        name: regionMatch[1].trim(),
        file: currentFile || 'unknown',
        line: i + 1
      });
    }
  }

  return regions;
}

// 如果直接运行此脚本
if (require.main === module)
{
  console.log('📂 使用配置的搜索目录:', SEARCH_DIRECTORIES);
  generateCodesModule();
}

module.exports = { generateCodesModule, findCsFiles, readAndMergeCsFiles, SEARCH_DIRECTORIES };
