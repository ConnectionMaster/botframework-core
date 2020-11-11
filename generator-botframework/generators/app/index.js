// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const Generator = require('yeoman-generator');
const path = require('path');
const { v4: uuidv4 } = require('uuid');

module.exports = class extends Generator {
    constructor(args, opts) {
        super(args, opts);

        this.argument('botName', {
            type: String,
            required: true
        });
    }

    // 1. initializing - Your initialization methods (checking current project state, getting configs, etc)
    // 2. prompting - Where you prompt users for options (where you’d call this.prompt())
    // 3. configuring - Saving configurations and configure the project (creating .editorconfig files and other metadata files)
    // 4. default - If the method name doesn’t match a priority, it will be pushed to this group.
    // 5. writing - Where you write the generator specific files (routes, controllers, etc)
    // 6. conflicts - Where conflicts are handled (used internally)
    // 7. install - Where installations are run (npm, bower)
    // 8. end - Called last, cleanup, say good bye, etc

    writing() {
        this._copyMicrosoftBotCoreProject();
        this._copyMicrosoftBotRuntimeCustomActionProject();
        this._copyBotProject();
        this._copySolutionFile();
    }

    _copyBotProject() {
        const botName = this.options.botName;

        this.fs.copyTpl(
            this.templatePath(path.join('botName','**')),
            this.destinationPath(botName),
            {
                botName
            }
        );

        this.fs.move(
            this.destinationPath(path.join(botName, 'botName.csproj')),
            this.destinationPath(path.join(botName, `${botName}.csproj`))
        );
    }

    _copyMicrosoftBotCoreProject() {
        this.fs.copy(
            this.templatePath(path.join('Microsoft.Bot.Core','**')),
            this.destinationPath('Microsoft.Bot.Core'));
    }

    _copyMicrosoftBotRuntimeCustomActionProject() {
        this.fs.copy(
            this.templatePath(path.join('Microsoft.Bot.Runtime.CustomAction','**')),
            this.destinationPath('Microsoft.Bot.Runtime.CustomAction'));
    }

    _copySolutionFile() {
        const botName = this.options.botName;
        const botProjectGuid = uuidv4().toUpperCase();
        const microsoftBotCoreProjectGuid = uuidv4().toUpperCase();
        const microsoftBotRuntimeCustomActionProjectGuid = uuidv4().toUpperCase();
        const solutionGuid = uuidv4().toUpperCase();

        this.fs.copyTpl(
            this.templatePath('botName.sln'),
            this.destinationPath(`${botName}.sln`),
            {
                botName,
                botProjectGuid,
                microsoftBotCoreProjectGuid,
                microsoftBotRuntimeCustomActionProjectGuid,
                solutionGuid
            }
        );
    }
};
