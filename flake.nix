{
  description = "NKK";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-26.05";
    flake-parts.url = "github:hercules-ci/flake-parts";
  };

  outputs = inputs@{ self, flake-parts, ... }:
    flake-parts.lib.mkFlake { inherit inputs; } {
      systems = [ "x86_64-linux" ];
      perSystem = { config, self', inputs', lib, pkgs, system, ... }: rec {
        packages = rec {
          nkk = pkgs.buildDotnetModule {
            pname = "NKK";
            version = "0.0.0+${inputs.self.dirtyShortRev or inputs.self.shortRev or "unknown"}";
            
            src = ./.;
            
            projectFile = "./NKK.csproj";
            nugetDeps = ./deps.json;
            
            GIT_COMMIT = inputs.self.rev or (lib.removeSuffix "-dirty" inputs.self.dirtyRev);
            GIT_COMMIT_SHORT = inputs.self.shortRev or (lib.removeSuffix "-dirty" inputs.self.dirtyShortRev);
            
            dotnet-sdk = pkgs.dotnet-sdk_10;
            dotnet-runtime = pkgs.dotnet-sdk_10;
            nativeBuildInputs = [
              pkgs.tailwindcss_4
              pkgs.which
            ];
          };
          
          default = nkk;
        };
        
        apps = {
          fetch-deps = {
            type = "app";
            program = "${pkgs.writeShellScript "fetch-deps" ''
              exec ${packages.nkk.passthru.fetch-deps} "$PWD/deps.json"
            ''}";
          };
        };
      };
      flake = {
        # The usual flake attributes can be defined here, including system-
        # agnostic ones like nixosModule and system-enumerating ones, although
        # those are more easily expressed in perSystem.

      };
    };
}
