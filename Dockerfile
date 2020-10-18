FROM registry.redhat.io/dotnet/dotnet-31-rhel7 AS build-env
USER 0
COPY . ./
RUN chown -R 1001:0 /opt/app-root && fix-permissions /opt/app-root
USER 1001
RUN /usr/libexec/s2i/assemble
CMD /usr/libexec/s2i/run

# Build runtime image
FROM registry.redhat.io/dotnet/dotnet-31-rhel7
USER 0
COPY --from=build-env /opt/app-root/app .
RUN chown -R 1001:0 /opt/app-root && fix-permissions /opt/app-root
USER 1001
ENTRYPOINT ["dotnet", "Chat.Web.dll"]
