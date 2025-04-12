// FileSystemExtension.js
(function (Scratch) {
  "use strict";

  if (!Scratch.extensions.unsandboxed) {
    throw new Error("FileSystem extension must run in unsandboxed mode");
  }

  const fs = require("fs").promises;
  const path = require("path");
  const https = require("https");

  class FileSystemExtension {
    constructor() {
      this.lastError = ""; // Храним последнюю ошибку
    }

    getInfo() {
      return {
        id: "filesystem",
        name: "Файловая система",
        blocks: [
          {
            opcode: "createFile",
            blockType: Scratch.BlockType.REPORTER,
            text: "создать файл [FILE_PATH] с содержимым [CONTENT]",
            arguments: {
              FILE_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
              CONTENT: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "Привет, мир!",
              },
            },
          },
          {
            opcode: "deleteFile",
            blockType: Scratch.BlockType.REPORTER,
            text: "удалить файл [FILE_PATH]",
            arguments: {
              FILE_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
            },
          },
          {
            opcode: "readFile",
            blockType: Scratch.BlockType.REPORTER,
            text: "прочитать файл [FILE_PATH]",
            arguments: {
              FILE_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
            },
          },
          {
            opcode: "updateFile",
            blockType: Scratch.BlockType.REPORTER,
            text: "изменить файл [FILE_PATH] на содержимое [CONTENT]",
            arguments: {
              FILE_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
              CONTENT: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "Новое содержимое",
              },
            },
          },
          {
            opcode: "createFolder",
            blockType: Scratch.BlockType.REPORTER,
            text: "создать папку [FOLDER_PATH]",
            arguments: {
              FOLDER_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame",
              },
            },
          },
          {
            opcode: "renameFileOrFolder",
            blockType: Scratch.BlockType.REPORTER,
            text: "переименовать [PATH] в [NEW_PATH]",
            arguments: {
              PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
              NEW_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/new-data.txt",
              },
            },
          },
          {
            opcode: "listFiles",
            blockType: Scratch.BlockType.REPORTER,
            text: "список файлов в [FOLDER_PATH]",
            arguments: {
              FOLDER_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame",
              },
            },
          },
          {
            opcode: "deleteAllFiles",
            blockType: Scratch.BlockType.REPORTER,
            text: "удалить все файлы в [FOLDER_PATH]",
            arguments: {
              FOLDER_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame",
              },
            },
          },
          {
            opcode: "copyFileOrFolder",
            blockType: Scratch.BlockType.REPORTER,
            text: "копировать [SOURCE_PATH] в [DEST_PATH]",
            arguments: {
              SOURCE_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
              DEST_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/backup.txt",
              },
            },
          },
          {
            opcode: "moveFileOrFolder",
            blockType: Scratch.BlockType.REPORTER,
            text: "переместить [SOURCE_PATH] в [DEST_PATH]",
            arguments: {
              SOURCE_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/data.txt",
              },
              DEST_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/backup/data.txt",
              },
            },
          },
          {
            opcode: "downloadFile",
            blockType: Scratch.BlockType.REPORTER,
            text: "скачать файл с URL [URL] в [DEST_PATH]",
            arguments: {
              URL: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "https://raw.githubusercontent.com/user/repo/main/game.txt",
              },
              DEST_PATH: {
                type: Scratch.ArgumentType.STRING,
                defaultValue: "C:/Users/Имя/AppData/Roaming/MyGame/game.txt",
              },
            },
          },
          {
            opcode: "getLastError",
            blockType: Scratch.BlockType.REPORTER,
            text: "последняя ошибка",
            arguments: {},
          },
        ],
      };
    }

    async _handleError(operation, callback) {
      try {
        this.lastError = "";
        const result = await callback();
        return result !== undefined ? result : "OK";
      } catch (err) {
        this.lastError = `${operation}: ${err.message}`;
        console.error(this.lastError);
        return this.lastError;
      }
    }

    async createFile({ FILE_PATH, CONTENT }) {
      return this._handleError("Создание файла", async () => {
        const fullPath = path.resolve(FILE_PATH);
        const dir = path.dirname(fullPath);
        await fs.mkdir(dir, { recursive: true });
        await fs.writeFile(fullPath, String(CONTENT), "utf8");
      });
    }

    async deleteFile({ FILE_PATH }) {
      return this._handleError("Удаление файла", async () => {
        const fullPath = path.resolve(FILE_PATH);
        const stat = await fs.stat(fullPath);
        if (!stat.isFile()) {
          throw new Error("Не файл");
        }
        await fs.unlink(fullPath);
      });
    }

    async readFile({ FILE_PATH }) {
      return this._handleError("Чтение файла", async () => {
        const fullPath = path.resolve(FILE_PATH);
        const stat = await fs.stat(fullPath);
        if (!stat.isFile()) {
          throw new Error("Файл не существует");
        }
        return await fs.readFile(fullPath, "utf8");
      });
    }

    async updateFile({ FILE_PATH, CONTENT }) {
      return this._handleError("Обновление файла", async () => {
        const fullPath = path.resolve(FILE_PATH);
        const stat = await fs.stat(fullPath);
        if (!stat.isFile()) {
          throw new Error("Файл не существует");
        }
        await fs.writeFile(fullPath, String(CONTENT), "utf8");
      });
    }

    async createFolder({ FOLDER_PATH }) {
      return this._handleError("Создание папки", async () => {
        const fullPath = path.resolve(FOLDER_PATH);
        await fs.mkdir(fullPath, { recursive: true });
      });
    }

    async renameFileOrFolder({ PATH, NEW_PATH }) {
      return this._handleError("Переименование", async () => {
        const fullPath = path.resolve(PATH);
        const newFullPath = path.resolve(NEW_PATH);
        await fs.rename(fullPath, newFullPath);
      });
    }

    async listFiles({ FOLDER_PATH }) {
      return this._handleError("Список файлов", async () => {
        const fullPath = path.resolve(FOLDER_PATH);
        const stat = await fs.stat(fullPath);
        if (!stat.isDirectory()) {
          throw new Error("Не папка");
        }
        const files = await fs.readdir(fullPath);
        return JSON.stringify(files);
      });
    }

    async deleteAllFiles({ FOLDER_PATH }) {
      return this._handleError("Удаление всех файлов", async () => {
        const fullPath = path.resolve(FOLDER_PATH);
        const stat = await fs.stat(fullPath);
        if (!stat.isDirectory()) {
          throw new Error("Не папка");
        }
        const files = await fs.readdir(fullPath);
        for (const file of files) {
          const filePath = path.join(fullPath, file);
          if ((await fs.stat(filePath)).isFile()) {
            await fs.unlink(filePath);
          }
        }
      });
    }

    async copyFileOrFolder({ SOURCE_PATH, DEST_PATH }) {
      return this._handleError("Копирование", async () => {
        const src = path.resolve(SOURCE_PATH);
        const dest = path.resolve(DEST_PATH);
        const stat = await fs.stat(src);
        await fs.mkdir(path.dirname(dest), { recursive: true });
        if (stat.isDirectory()) {
          await this._copyDir(src, dest);
        } else {
          await fs.copyFile(src, dest);
        }
      });
    }

    async _copyDir(src, dest) {
      await fs.mkdir(dest, { recursive: true });
      const entries = await fs.readdir(src, { withFileTypes: true });
      for (const entry of entries) {
        const srcPath = path.join(src, entry.name);
        const destPath = path.join(dest, entry.name);
        if (entry.isDirectory()) {
          await this._copyDir(srcPath, destPath);
        } else {
          await fs.copyFile(srcPath, destPath);
        }
      }
    }

    async moveFileOrFolder({ SOURCE_PATH, DEST_PATH }) {
      return this._handleError("Перемещение", async () => {
        const src = path.resolve(SOURCE_PATH);
        const dest = path.resolve(DEST_PATH);
        await fs.mkdir(path.dirname(dest), { recursive: true });
        await fs.rename(src, dest);
      });
    }

    async downloadFile({ URL, DEST_PATH }) {
      return this._handleError("Скачивание файла", async () => {
        const fullPath = path.resolve(DEST_PATH);
        await fs.mkdir(path.dirname(fullPath), { recursive: true });
        return new Promise((resolve, reject) => {
          https.get(URL, (res) => {
            if (res.statusCode !== 200) {
              reject(new Error(`HTTP ${res.statusCode}`));
              return;
            }
            const file = require("fs").createWriteStream(fullPath);
            res.pipe(file);
            file.on("finish", () => {
              file.close();
              resolve("OK");
            });
            file.on("error", (err) => {
              reject(err);
            });
          }).on("error", reject);
        });
      });
    }

    getLastError() {
      return this.lastError || "Нет ошибок";
    }
  }

  Scratch.extensions.register(new FileSystemExtension());
})(Scratch);
